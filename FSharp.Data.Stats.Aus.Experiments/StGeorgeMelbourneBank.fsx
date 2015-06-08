(*** hide ***)
#load "packages/FsLab/FsLab.fsx"

open System
open System.IO
open System.Linq    
open FSharp.Charting

let transFileName = __SOURCE_DIRECTORY__ + @"\data\trans.csv"
let outputFileNane = __SOURCE_DIRECTORY__ + @"\data\transOut.csv"

type BankTransType = | VisaPurchase | VisaPurchaseOSeas | CashDeposit | InternetDeposit | EftposPurchase | InternetWithdrawal | UnknownBankTransType of string 

type BankTrans = { Date : DateTime;                      
                   RawDescription : string;
                   Description : string;                                   
                   Debit : double;        
                   Credit : double;
                   Balance : double;
                   TransType : string; }

let getTransAsType (line : string) =        
        match line.[11..32].Trim() with
        | s when s.Contains("Cash Deposit") = true -> CashDeposit
        | s when s.Contains("Visa Purchase") = true -> VisaPurchase
        | s when s.Contains("Visa Purchase O/Seas") = true -> VisaPurchaseOSeas    
        | s when s.Contains("Eftpos Purchase") = true -> EftposPurchase   
        | s when s.Contains("Internet Deposit") = true -> InternetDeposit   
        | s when s.Contains("Internet Withdrawal") = true -> InternetWithdrawal           
        | s -> UnknownBankTransType(s)

let getTransAsString (line : string) = line.[11..32].Trim() |> (fun s -> if (s.IndexOf(',') > 0) then s.Remove(s.IndexOf(',')) else s)  
 
let removeTrailers (s : string) = s.LastIndexOf("  ") |> (fun index -> if index > 0 then s.Remove(index).Trim() else s.Trim())
        
let getDesc (line : string) (t : BankTransType) =
        let fields = line.Split(',') |> Array.map(fun field -> field.Trim())               
        (match t with        
         | VisaPurchase -> fields.[1].[36 .. 56].Trim() 
         | VisaPurchaseOSeas -> fields.[1].[40 .. ] |> (fun s -> s.Remove(s.IndexOf(' '))) |> removeTrailers
         | InternetDeposit -> fields.[1].[41 .. ].Trim() 
         | EftposPurchase -> fields.[1].[41 .. 64].Trim()        
         | CashDeposit -> "Cash Deposit"
         | InternetWithdrawal -> fields.[1].[41 .. ].Trim()
         | UnknownBankTransType s -> s) |> removeTrailers                

let mapLineToBankTrans (line : string) = let tryParseDbl (s : string) = if (s.Trim() = String.Empty) then 0.0 else Double.Parse(s.Trim()) 
                                         let fields = line.Split(',') |> Array.map(fun field -> field.Trim())                                           
                                         { BankTrans.Date = DateTime.Parse(fields.[0]); 
                                                     Description = getTransAsType line |> getDesc line; 
                                                     TransType = getTransAsString line;
                                                     Debit = tryParseDbl fields.[2]; Credit = tryParseDbl fields.[3];
                                                     Balance = tryParseDbl fields.[4];                                                       
                                                     RawDescription = line } 

let printTotals (kvps : (string * double) seq) = kvps |> Seq.iteri(fun i (k, v) -> printfn "%d. %s - %.2f" (i + 1) k v)                                                                                
let filterCredits (ts : BankTrans seq) = ts |> Seq.filter(fun t -> t.Credit > 0.0)
let filterDebits (ts : BankTrans seq) = ts |> Seq.filter(fun t -> t.Credit > 0.0)
let sumCredits (ts : BankTrans seq) =  ts |> Seq.sumBy(fun t -> t.Credit)
let sumDebits (ts : BankTrans seq) =  ts |> Seq.sumBy(fun t -> t.Debit)

let trans = File.ReadAllLines transFileName |> Seq.skip 1 |> Seq.toArray |> Array.map mapLineToBankTrans

                                            
// Debits vs Credits                                           
[ ("Debits", trans |> Array.sumBy(fun t -> t.Debit)); ("Credits", trans |> Array.sumBy(fun t -> t.Credit)) ] |> Chart.Column

// Credit totals by type
trans |> filterCredits |> Seq.groupBy(fun t -> t.TransType) |> Seq.map(fun (g, ts) -> g, sumCredits ts) |> printTotals
trans |> filterDebits |> Seq.groupBy(fun t -> t.TransType) |> Seq.map(fun (g, ts) -> g, sumDebits ts) |> printTotals

// Print debit totals by description 
trans |> filterDebits |> Seq.groupBy(fun t -> t.Description) |> Seq.map(fun (d, ts) -> d, ts |> sumDebits) |> Seq.sortBy(fun (_, debit) -> -debit) |> printTotals
                                          
// Debit Activity by day
trans |> filterDebits |> Seq.groupBy(fun t -> t.Date) |> Seq.map(fun (dt, ts) -> dt, ts |> sumDebits) |> Chart.Line 

// Debit Activity by month
trans |> Seq.filter(fun t -> t.Debit > 0.0) |> Seq.groupBy(fun t -> new DateTime(t.Date.Year, t.Date.Month, 1)) |> Seq.map(fun (m, ts) -> m, ts |> sumDebits) |> Chart.Bar 
 
let getDirection (t : BankTrans) = if (t.Debit > 0.0) then "Out" else "In"
let header = sprintf "Date|Direction|Tranaction Type|Description|Debits|Credits|Balance|RawDescription"
let lines = trans |> Seq.map(fun t -> sprintf "%s|%s|%s|%s|%0.2f|%0.2f|%0.2f|%s" (t.Date.ToShortDateString()) (getDirection t) t.TransType t.Description t.Debit t.Credit t.Balance t.RawDescription)
                  |> Seq.toArray

(outputFileNane, Array.append [| header |] lines)
|> File.WriteAllLines