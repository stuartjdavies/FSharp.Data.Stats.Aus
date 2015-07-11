(*** hide ***)
#load "Setup.fsx"
#r "Microsoft.Office.Interop.Excel.dll"

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
open Microsoft.Office.Interop.Excel


let ASX_DATA_DIR = __SOURCE_DIRECTORY__ + "\\Data\\ASX\\"
let DATA_DIR = __SOURCE_DIRECTORY__ + "\\Data\\ASX\\"
let ASX_LISTED_COMPANIES_URL = "http://www.asx.com.au/asx/research/ASXListedCompanies.csv"
     
type ASXCompany = { CompanyName : String; ASXCode : String; GICSIndustryGroup : String }
type StockItem = { ASXCode : String; Date : DateTime; Open : Single; High : Single; 
                   Low : Single; Close : Single; Volume : Single; AdjClose : Single }
     
if (Directory.Exists(DATA_DIR) = false) then
    Directory.CreateDirectory(DATA_DIR) |> ignore

if (Directory.Exists(ASX_DATA_DIR) = false) then
    Directory.CreateDirectory(ASX_DATA_DIR)  |> ignore

let downloadPageAsString(url : string)= (new WebClient()).DownloadString url   

let downloadCompanyData (codes : ASXCompany array) = 
             codes |> Array.map(fun c -> c.ASXCode, sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s.AX" c.ASXCode)                          
                   |> Array.Parallel.mapi(fun i (code, url) -> try
                                                                 let data = downloadPageAsString url
                                                                 let fileName = (sprintf "%s%s.csv" ASX_DATA_DIR code )
                                                                 File.WriteAllText(fileName, data)
                                                                 //printfn "Written file: i - %d - code %s FileName - %s" (i + 1) code fileName 
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

let getASXCodeFromFileName (fileName : string) = fileName.[(fileName.LastIndexOf("\\") + 1) ..].Replace(".csv","") 


let getFirstLine code fileName = File.ReadLines(fileName) |> Seq.skip(1) |> Seq.head |> toStockItem code
let getStockHistory code fileName = File.ReadLines(fileName).ToArray().[1..] |>  Array.map(fun item -> toStockItem code item)
 


let ausCompanies = downloadPageAsString ASX_LISTED_COMPANIES_URL
                   |> (fun (s : string) -> s.Split('\n'))
                   |> (fun lines -> lines.[3..])
                   |> Array.filter(fun line -> line.Trim() <> String.Empty)
                   |> Array.map(fun line -> let vs = line.Split(',')
                                            { ASXCompany.CompanyName = vs.[0].Replace("\"","").Replace("\r", String.Empty);
                                                         ASXCode =vs.[1];
                                                         GICSIndustryGroup=vs.[2].Replace("\"","").Replace("\r", String.Empty); })                                                                                       

///
/// Write all asx companies to directory
/// 
do               
    ausCompanies |> Seq.mapi(fun i c -> (i / 200), c)
                 |> Seq.groupBy(fun (batch, _) -> batch)
                 |> Seq.toArray
                 |> Array.iter(fun (batch, xs) -> xs |> Seq.map(fun (_, ys) -> ys) |> Seq.toArray
                                                     |> downloadCompanyData |> ignore
                                                  printfn "Downloaded batch %d" batch)    
                                                                                                                                                                                                                   
///
/// Test Array.Parallel vs Array.map
///
do                         
     let fileCount = Directory.GetFiles(ASX_DATA_DIR).Length
                                                                                               
     let sw1 = new Stopwatch()
     sw1.Start()
     let numberOfStocksRead = Directory.GetFiles(ASX_DATA_DIR) 
                              |> Array.mapi (fun i (fileName : string) -> let code = getASXCodeFromFileName fileName 
                                                                          printfn "Processed %s, %d" code i
                                                                          (getStockHistory code fileName).Length)                                 
                              |> Array.sum

     sw1.Stop()         
                                                                                               
     let sw2 = new Stopwatch()
     sw2.Start()
     let numberOfStocksRead2 = Directory.GetFiles(ASX_DATA_DIR) 
                               |> Array.Parallel.mapi (fun i (fileName : string) -> let code = getASXCodeFromFileName fileName
                                                                                    printfn "Processed %d" i
                                                                                    File.ReadLines(fileName).ToArray().[1..]
                                                                                    |> Array.Parallel.map(fun item -> toStockItem code item)
                                                                                    |> (fun items -> items.Length))                                 
                               |> Array.sum

     sw2.Stop()         
     printfn "Seq Total minutes elapsed  %.2f, Files - %d numberOfStocks %d" sw1.Elapsed.TotalSeconds fileCount numberOfStocksRead
     printfn "Parallel Total minutes elapsed  %.2f, Files - %d numberOfStocks %d" sw2.Elapsed.TotalSeconds fileCount numberOfStocksRead2
     ()

let stocksOrderedByClosePrice = Directory.GetFiles(ASX_DATA_DIR) 
                                |> Array.mapi (fun i (fileName : string) -> let code = getASXCodeFromFileName fileName
                                                                            printfn "Processed %s, %d" code i
                                                                            getFirstLine code fileName) 


let stocks = stocksOrderedByClosePrice |> Array.map(fun item -> printfn "Processed %s" item.ASXCode
                                                                let m = ausCompanies |> Array.find(fun c -> c.ASXCode=item.ASXCode)
                                                                (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, item.Close))

//
// Get higest top 100 company prices
//
do
    printfn "Top companies industry"
    printfn ""
    stocks.[0..100] |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)
    ()

//
// Get number of companies by industry
//
do
    printfn "Number of companies by industry"
    printfn ""

    stocks |> Seq.groupBy(fun (_,_,g,_) -> g) 
           |> Seq.map(fun (g, xs) -> g, xs |> Seq.length)
           |> Seq.sortBy(fun (_, l) -> -l)
           |> Seq.iteri(fun i (g, l) -> printfn "%d. %s %d" (i + 1) (g.Replace("\r","")) l)
    ()

//
// Get average stock prices by industry
//
do
    printfn "Average stock prices by industry"
    printfn ""

    stocks |> Seq.groupBy(fun (_,_,g,_) -> g) 
           |> Seq.map(fun (g, items) -> g, items |> Seq.averageBy(fun (_,_,_,close) -> close))
           |> Seq.sortBy(fun (_, l) -> -l)
           |> Seq.iteri(fun i (g, l) -> printfn "%d. %s %0.2f" (i + 1) (g.Replace("\r","")) l)

//
// Get List of closing prices for Pharmaceutical & Biotechnology companies
//
do
    printfn "List of closing prices for Pharmaceuticals & Biotechnology"
    printfn ""
    stocks |> Array.filter(fun (_,_,g,_) -> g = "Pharmaceuticals & Biotechnology") 
           |> Array.sortBy(fun (_,_,_,close) -> -close)
           |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)
    ()

// 
// Get list of household and personal products
//
do
    printfn "Insurance"
    printfn ""
    stocks |> Array.filter(fun (_,_,g,_) -> g = "Insurance") 
           |> Array.sortBy(fun (_,_,_,close) -> -close)
           |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)
    ()

//
// Find the biggest increase in last 30 days                             
//
do
    printfn "Household & Personal Products"
    printfn ""
    stocks |> Array.filter(fun (_,_,g,_) -> g = "Household & Personal Products") 
           |> Array.sortBy(fun (_,_,_,close) -> -close)
           |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)
    ()

do
    printfn "Food"
    printfn ""
    stocks |> Array.filter(fun (_,_,g,_) -> g = "Food") 
           |> Array.sortBy(fun (_,_,_,close) -> -close)
           |> Array.iteri(fun i (code, name, group, close) -> printfn "%d. %s, %s, %s, %.2f" (i + 1) code name group close)
    ()

//
// Find the greatest closing price changes in the last 2 months 
//
do  
    let priceChanges = Directory.GetFiles(ASX_DATA_DIR) 
                       |> Array.mapi (fun i (fileName : string) -> 
                                            let code = getASXCodeFromFileName fileName
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
    
    // Find the largest increase in the last 2 months    
    do 
        priceChanges
        |> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                            (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
        |> Array.iteri (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                         printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
        ()

    // Find the largest increases in the last 2 months for Software & Services   
    do
        priceChanges   
        |> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                            (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
        |> Array.filter (fun (_, _, g, _, _, _) -> g = "Software & Services")        
        |> Array.iteri (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                         printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
        ()

    // Find the largest increases in the last 2 months for Pharmaceuticals & Biotechnology
    do
        priceChanges   
        |> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                            (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
        |> Array.filter (fun (_, _, g, _, _, _) -> g = "Pharmaceuticals & Biotechnology")        
        |> Array.iteri (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                         printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
        ()

    // Find the largest increases in the last 2 months for Pharmaceuticals & Biotechnology
    do
        priceChanges   
        |> Array.map (fun (c,c1,c2,diff) -> let m = ausCompanies |> Array.find(fun item -> item.ASXCode=c)
                                            (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, c1,c2,diff))
        |> Array.filter (fun (_, _, g, _, _, _) -> g = "Software & Services")        
        |> Array.iteri (fun i (c, n, g, c1, c2, diff) -> let increase = ((c1 - c2) / c1) * 100.0f
                                                         printfn "%d. %s %s %s %.2f %.2f %.2f  %.2f percent" (i + 1) c n g c1 c2 diff increase)
        ()

// 
// Table Columns 
// 1. ASXCode
// 2. Company Name1
// 3. Industry group
// 4.  Current price
// 5.  1/2/3/4/5/6 month and 1/2 year 
//         - Price change in month
//         - Price change against current
//         - Average price
//         - Std Deviation
//         - Min in month
//         - Max in month
//         - Largest window change
// 6. Order By Highest close prices.
// 

let  getFirstRowsForAllCompanies (cs : ASXCompany array) = 
                cs |> Array.Parallel.mapi (fun i c -> let fileName = sprintf "%s%s.csv" ASX_DATA_DIR c.ASXCode 
                                                      printfn "Processed %s, %d, %s" c.ASXCode i fileName
                                                      getFirstLine c.ASXCode fileName) 




// Save as html / upload to aws.
type ASXSummaryTableItem = {
        ASXCode : String;
        CompanyName : String;
        IndustryGroup : String;
        LastDate : DateTime; 
        LastOpen : Single; 
        LastHigh : Single; 
        LastLow : Single; 
        LastClose : Single; 
        LastVolume : Single; 
        LastAdjClose : Single                
}

let writeSummaryTableToExcel (items : ASXSummaryTableItem array) =      
        let app = new ApplicationClass(Visible = true) 
        let workbook = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet) 
        
        let worksheet = (workbook.Worksheets.[1] :?> Worksheet)
        worksheet.Name <- "ASX Company Summary"

        items |> Array.iteri(fun i r -> worksheet.Range("A" + (i + 1).ToString(), "J" + (i + 1).ToString()).Value2 <- 
                                        [| r.ASXCode; 
                                           r.CompanyName;
                                           r.IndustryGroup;
                                           r.LastDate.ToShortDateString();
                                           r.LastOpen.ToString();
                                           r.LastHigh.ToString();
                                           r.LastLow.ToString();
                                           r.LastClose.ToString();
                                           r.LastVolume.ToString();
                                           r.LastAdjClose.ToString(); |] )

        let columnText = [| "ASXCode"; "CompanyName"; "IndustryGroup"; "Last Date";
                            "Last Open"; "LastHigh"; "Last Low"; "Last Close"; "LastVolume"; "Last AdjClose"; |]

        let range = worksheet.get_Range(sprintf "A1:J%d" (items.Length + 1))
        worksheet.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange,range, Type.Missing,XlYesNoGuess.xlGuess, Type.Missing, Type.Missing).Name <- "ASXSummary"
        worksheet.ListObjects.[  "ASXSummary" ].TableStyle <- "TableStyleMedium2";
        worksheet.Range("A1", "J1").Value2 <- columnText
        range.Columns.AutoFit()


        
// do         
let firstRowsForAllAsxCompanies = ausCompanies |> getFirstRowsForAllCompanies  
          
let table = ausCompanies |> Array.map(fun item -> let firstRow = firstRowsForAllAsxCompanies |> Array.find(fun r -> item.ASXCode = r.ASXCode) 
                                                  { ASXSummaryTableItem.ASXCode = item.ASXCode;
                                                                        CompanyName = item.CompanyName;
                                                                        IndustryGroup = item.GICSIndustryGroup;
                                                                        LastDate = firstRow.Date; 
                                                                        LastOpen = firstRow.Open;
                                                                        LastHigh = firstRow.High;
                                                                        LastLow = firstRow.Low;
                                                                        LastClose = firstRow.Close;
                                                                        LastVolume = firstRow.Volume;
                                                                        LastAdjClose = firstRow.AdjClose } )
                        |> Array.sortBy(fun item -> -item.LastClose)                                                  
     
     
table |> writeSummaryTableToExcel |> ignore                                                   
//     ()