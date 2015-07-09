(*** hide ***)
#load "Setup.fsx"

// Get a list of companies 
// http://www.asx.com.au/asx/research/ASXListedCompanies.csv

open System
open System.IO
open System.Linq
open System.Collections.Generic
open System.Net
open FSharp.Data
open FSharp.Charting
open System.Text.RegularExpressions
open HtmlAgilityPack
open Setup
open Deedle
open System.Diagnostics
open Microsoft.FSharp.Linq

let downloadPageAsString(url : string)= (new WebClient()).DownloadString url   

let rawASXListedCompanies = downloadPageAsString "http://www.asx.com.au/asx/research/ASXListedCompanies.csv" 
let s = downloadPageAsString "http://ichart.finance.yahoo.com/table.csv?s=WBC.AX"

type ASXCompany = { CompanyName : String; ASXCode : String; GICSIndustryGroup : String }
type StockItem = { ASXCode : String; Date : DateTime; Open : Single; High : Single; 
                   Low : Single; Close : Single; Volume : Single; AdjClose : Single }
                                                        
let ausCompanies = rawASXListedCompanies
                   |> (fun (s : string) -> s.Split('\n'))
                   |> (fun lines -> lines.[3..])
                   |> Array.filter(fun line -> line.Trim() <> String.Empty)
                   |> Array.map(fun line -> let vs = line.Split(',')
                                            { ASXCompany.CompanyName = vs.[0].Replace("\"","").Replace("\r", String.Empty);
                                                         ASXCode =vs.[1];
                                                         GICSIndustryGroup=vs.[2].Replace("\"","").Replace("\r", String.Empty); })                                                                                       
                   
ausCompanies |> Array.map(fun c -> c.ASXCode, sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s.AX" c.ASXCode)
             |> Array.mapi(fun i (code, url) -> try
                                                    let data = downloadPageAsString url
                                                    File.WriteAllText ((sprintf "c:\\ASX\\%s.csv" code), data)
                                                    printfn "Written file - %d, %s" i code
                                                with 
                                                | ex -> printfn "Exception code - %d, %s" i code)
             
let toStockItem asxCode (line: string) = line.Split(',')
                                         |> (fun fields -> {StockItem.Date = DateTime.Parse(fields.[0]);
                                                             ASXCode = asxCode;
                                                             Open = Single.Parse(fields.[1]);
                                                             High = Single.Parse(fields.[2]);
                                                             Low = Single.Parse(fields.[3]);
                                                             Close = Single.Parse(fields.[4]);
                                                             Volume = Single.Parse(fields.[5]);
                                                             AdjClose = Single.Parse(fields.[6])})

let getASXCodeFromFileName (nm : string) = nm.[7..].Replace(".csv","")

let getFirstLine code fileName = File.ReadLines(fileName) |> Seq.skip(1) |> Seq.head |> toStockItem code
let getStockHistory code fileName = File.ReadLines(fileName).ToArray().[1..] |>  Array.map(fun item -> toStockItem code item)
 

let stocksOrderedByClosePrice = Directory.GetFiles("c:\\ASX\\") 
                                |> Array.map (fun (fileName : string) -> let code = fileName.[7..].Replace(".csv","") 
                                                                         getFirstLine code fileName) 


                         
let fileCount = Directory.GetFiles("c:\\ASX\\").Length
                                                                                               
let sw1 = new Stopwatch()
sw1.Start()
let numberOfStocksRead = Directory.GetFiles("c:\\ASX\\") 
                         |> Array.mapi (fun i (fileName : string) -> let code = fileName.[7..].Replace(".csv","") 
                                                                     printfn "Processed %s, %d" code i
                                                                     (getStockHistory code fileName).Length)                                 
                         |> Array.sum

sw1.Stop()         
                                                                                               
let sw2 = new Stopwatch()
sw2.Start()
let numberOfStocksRead2 = Directory.GetFiles("c:\\ASX\\") 
                          |> Array.Parallel.mapi (fun i (fileName : string) -> let code = fileName.[7..].Replace(".csv","") 
                                                                               printfn "Processed %d" i
                                                                               File.ReadLines(fileName).ToArray().[1..]
                                                                               |> Array.Parallel.map(fun item -> toStockItem code item)
                                                                               |> (fun items -> items.Length))                                 
                          |> Array.sum

sw2.Stop()         
printfn "Seq Total minutes elapsed  %.2f, Files - %d numberOfStocks %d" sw1.Elapsed.TotalSeconds fileCount numberOfStocksRead
printfn "Parallel Total minutes elapsed  %.2f, Files - %d numberOfStocks %d" sw2.Elapsed.TotalSeconds fileCount numberOfStocksRead2
                              
stocksOrderedByClosePrice.[0..100] |> Array.map(fun item -> let m = ausCompanies |> Array.find(fun c -> c.ASXCode=item.ASXCode)
                                                            (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, item.Close))

let stocks = stocksOrderedByClosePrice |> Array.map(fun item -> let m = ausCompanies |> Array.find(fun c -> c.ASXCode=item.ASXCode)
                                                                (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, item.Close))

printfn "Top companies industry"
printfn ""
stocks.[0..100] |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)

printfn "Number of companies by industry"
printfn ""

stocks |> Seq.groupBy(fun (_,_,g,_) -> g) 
       |> Seq.map(fun (g, xs) -> g, xs |> Seq.length)
       |> Seq.sortBy(fun (_, l) -> -l)
       |> Seq.iteri(fun i (g, l) -> printfn "%d. %s %d" (i + 1) (g.Replace("\r","")) l)

printfn "Average stock prices by industry"
printfn ""

stocks |> Seq.groupBy(fun (_,_,g,_) -> g) 
       |> Seq.map(fun (g, items) -> g, items |> Seq.averageBy(fun (_,_,_,close) -> close))
       |> Seq.sortBy(fun (_, l) -> -l)
       |> Seq.iteri(fun i (g, l) -> printfn "%d. %s %0.2f" (i + 1) (g.Replace("\r","")) l)

printfn "List of closing prices for Pharmaceuticals & Biotechnology"
printfn ""
stocks |> Array.filter(fun (_,_,g,_) -> g = "Pharmaceuticals & Biotechnology") 
       |> Array.sortBy(fun (_,_,_,close) -> -close)
       |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)

printfn "Household & Personal Products"
printfn ""
stocks |> Array.filter(fun (_,_,g,_) -> g = "Household & Personal Products") 
       |> Array.sortBy(fun (_,_,_,close) -> -close)
       |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)

// Find the biggest increase in last 30 days                              
printfn "Household & Personal Products"
printfn ""
stocks |> Array.filter(fun (_,_,g,_) -> g = "Household & Personal Products") 
       |> Array.sortBy(fun (_,_,_,close) -> -close)
       |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)


// Find he
let priceChanges = Directory.GetFiles("c:\\ASX\\") 
                   |> Array.mapi (fun i (fileName : string) -> 
                                            let code = fileName.[7..].Replace(".csv","") 
                                            let rows = File.ReadLines(fileName).ToArray().[1..] |> Array.map (fun item -> toStockItem code item)                                                      
                                            printfn "Processed %d" i   
                                            if (rows.Length >= 60) then                                         
                                                Some (code, rows.[0].Close, rows.[59].Close, rows.[0].Close - rows.[59].Close)
                                            else
                                                None)
                   |> Array.filter(fun row -> match row with
                                              | Some _ -> true
                                              | None -> false)
                   |> Array.map(fun row -> row.Value)
                   |> Array.sortBy(fun (_, _, _, diff) -> -diff)      
                             
priceChanges.[0..30] |> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                                         (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
                     |> Array.mapi (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                                     printfn "%d. %s %s %s %.2f %.2f %.2f %.2f percent" (i + 1) c n g c1 c2 diff increase)
 
priceChanges
|> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                    (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
|> Array.mapi (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
                                                                                        

priceChanges   
|> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                    (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
|> Array.filter (fun (_, _, g, _, _, _) -> g = "Software & Services")
|> (fun xs -> xs)
|> Array.mapi (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
    
// Graph of stddev
// Top 10 increases
// Top 10 decreases
// Top 10 share prices
// Top 10 Pharmaceuticals & Biotechnology
// Top 10 Technology Hardware & Equipment
// Top 10 Software & Services

//* 100 / 60 

            