(*** hide ***)
#load "Setup.fsx"
#r "Microsoft.Office.Interop.Excel.dll"

open System
open System.IO
open System.Linq
open System.Collections.Generic
open System.Net
open FSharp.Data
open XPlot.GoogleCharts
open System.Text.RegularExpressions
open HtmlAgilityPack
open Setup
open Deedle
open System.Diagnostics
open Microsoft.FSharp.Linq
open Microsoft.Office.Interop.Excel
open Setup

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
                                                               | ex -> printfn "Exception code - %d, %s, %s" i code ex.Message)
                                                                                                                                                              

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
                                                                                                                                                                                                                   

let stocksOrderedByClosePrice = Directory.GetFiles(ASX_DATA_DIR) 
                                |> Array.mapi (fun i (fileName : string) -> let code = getASXCodeFromFileName fileName
                                                                            printfn "Processed %s, %d" code i
                                                                            getFirstLine code fileName) 


let stocks = stocksOrderedByClosePrice |> Array.map(fun item -> printfn "Processed %s" item.ASXCode
                                                                let m = ausCompanies |> Array.find(fun c -> c.ASXCode=item.ASXCode)
                                                                (m.ASXCode, m.CompanyName, m.GICSIndustryGroup, item.Close))

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
type ASXSummaryTableItem = {
        ASXCode : String;
        CompanyName : String;
        IndustryGroup : String;
        // OneDayClosePriceDiff : Single;
        // FiveDayClosePriceDiff : Single;
        // OneMonthClosePriceDiff : Single
        // OneYearClosePriceDiff : Single;
        // FiveYearClosePriceChange : Single;                 
        LastDate : DateTime; 
        LastOpen : Single; 
        LastHigh : Single; 
        LastLow : Single; 
        LastClose : Single; 
        LastVolume : Single; 
        LastAdjClose : Single;
        FirstDate : DateTime;
        FirstOpen : Single;
        FirstHigh : Single;
        FirstLow : Single;
        FirstClose : Single;
        FirstVolume : Single;
        FirstAdjClose : Single;
        MaxCloseDate : DateTime;
        MaxClose : Single;
        MinCloseDate : DateTime;
        MinClose : Single;
        AsxDetailsUrl : String;  
        YahooDetailsUrl : String;  
        HeadOfficeState : String;                    
}

let getStockHistoryStats (c : ASXCompany) =                                   
                let fileName = (sprintf "%s%s.csv" ASX_DATA_DIR c.ASXCode)
                let rows = File.ReadLines(fileName).ToArray().[1..] |> Array.map (fun item -> toStockItem c.ASXCode item)                                                      
                
                let firstRow = rows.[0]
                let lastRow = rows |> Seq.last 
                let maxClose = rows |> Array.maxBy(fun item -> item.Close)
                let minClose = rows |> Array.maxBy(fun item -> item.Close)

                let yahooUrl = (sprintf "https://au.finance.yahoo.com/q/pr?s=%s.AX" c.ASXCode)
                let html = downloadPageAsString yahooUrl
                let state = match html with
                            | _ when (html.IndexOf("NSW ", StringComparison.Ordinal) > 1) -> "NSW"
                            | _ when (html.IndexOf("ACT ", StringComparison.Ordinal) > 1) -> "ACT"
                            | _ when (html.IndexOf("VIC ", StringComparison.Ordinal) > 1) -> "VIC"
                            | _ when (html.IndexOf("QLD ", StringComparison.Ordinal) > 1) -> "QLD"
                            | _ when (html.IndexOf("WA ", StringComparison.Ordinal) > 1) -> "WA"
                            | _ when (html.IndexOf("NT ", StringComparison.Ordinal) > 1) -> "NT"                                                       
                            | _ -> "Unknown"
                
                printfn "Processed %s" c.ASXCode                                                                                                                                                           
                { ASXSummaryTableItem.ASXCode = c.ASXCode;
                                      CompanyName = c.CompanyName;
                                      IndustryGroup = c.GICSIndustryGroup;
                                      LastDate = firstRow.Date; 
                                      LastOpen = firstRow.Open;
                                      LastHigh = firstRow.High;
                                      LastLow = firstRow.Low;
                                      LastClose = firstRow.Close;
                                      LastVolume = firstRow.Volume;
                                      LastAdjClose = firstRow.AdjClose;
                                      FirstDate = lastRow.Date;
                                      FirstOpen = lastRow.Open;
                                      FirstHigh = lastRow.High;
                                      FirstLow = lastRow.Low;
                                      FirstClose = lastRow.Close;
                                      FirstVolume = lastRow.Volume;
                                      FirstAdjClose = lastRow.AdjClose;
                                      MaxCloseDate = maxClose.Date;
                                      MaxClose = maxClose.Close;
                                      MinCloseDate = minClose.Date;
                                      MinClose = minClose.Close;
                                      AsxDetailsUrl = (sprintf "http://www.asx.com.au/asx/research/company.do#!/%s/details" c.ASXCode);
                                      YahooDetailsUrl = yahooUrl; 
                                      HeadOfficeState = state; }
                                                 
let writeSummaryTableToExcel (items : ASXSummaryTableItem array) =      
        let app = new ApplicationClass(Visible = true) 
        let workbook = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet) 
        
        let worksheet = (workbook.Worksheets.[1] :?> Worksheet)
        worksheet.Name <- "ASX Company Summary"

        items |> Array.iteri(fun i r -> worksheet.Range("A" + (i + 1).ToString(), "X" + (i + 1).ToString()).Value2 <- 
                                        [| r.ASXCode; 
                                           r.CompanyName;
                                           r.IndustryGroup;
                                           r.LastDate.ToShortDateString();
                                           r.LastOpen.ToString();
                                           r.LastHigh.ToString();
                                           r.LastLow.ToString();
                                           r.LastClose.ToString();
                                           r.LastVolume.ToString();
                                           r.LastAdjClose.ToString(); 
                                           r.FirstDate.ToShortDateString();
                                           r.FirstOpen.ToString();
                                           r.FirstHigh.ToString() ;
                                           r.FirstLow.ToString();
                                           r.FirstClose.ToString();
                                           r.FirstVolume.ToString();
                                           r.FirstAdjClose.ToString();
                                           r.MaxCloseDate.ToShortDateString();
                                           r.MaxClose.ToString();
                                           r.MinCloseDate.ToShortDateString();
                                           r.MinClose.ToString();
                                           r.AsxDetailsUrl;
                                           r.YahooDetailsUrl;
                                           r.HeadOfficeState |] )

        let columnText = [| "ASXCode"; "Company Name"; "Industry Group"; "Last Date";
                            "Last Open"; "Last High"; "Last Low"; "Last Close"; "Last Volume"; "Last Adj Close"; 
                            "First Date"; "First Open"; "First High"; "First Low"; "First Close"; 
                            "First Volumn";  "First AdjClose"; "Max Close Date"; "Max Close"; "Min Close Date"; "Min Close"; "Asx Details Url"; "Yahoo Details Url"; "Head Office State"
                             |]

        let range = worksheet.get_Range(sprintf "A1:X%d" (items.Length + 1))
        worksheet.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange,range, Type.Missing,XlYesNoGuess.xlGuess, Type.Missing, Type.Missing).Name <- "ASXSummary"
        worksheet.ListObjects.[  "ASXSummary" ].TableStyle <- "TableStyleMedium2";
        worksheet.Range("A1", "X1").Value2 <- columnText
        range.Columns.AutoFit() |> ignore
        worksheet.SaveAs((sprintf "%s\\website\\ASXSummary.xls" __SOURCE_DIRECTORY__)) |> ignore
        
                                                                                                                                   
let table = ausCompanies |> Array.filter(fun item -> File.Exists( sprintf "%s%s.csv" ASX_DATA_DIR item.ASXCode) = true)
                         |> Array.Parallel.map getStockHistoryStats                                                
                         |> Array.sortBy(fun item -> -item.LastClose)                                                  
     
table |> writeSummaryTableToExcel |> ignore      

let ``Number of companies by industry``() = 
        stocks |> Seq.groupBy(fun (_,_,g,_) -> g) 
               |> Seq.map(fun (g, xs) -> g, xs |> Seq.length)
               |> Seq.sortBy(fun (_, l) -> -l) 
               |> Seq.toArray  
                                  

[ Title "Australian ASX Summary"
  H1("Australian ASX Summary")
  H2 "Introduction"
  P "Contains a brief analysis of all stocks on the asx"
  H2 "Key Points"         
  List [ "None at the moment" ] 
  H2 "Graphs"        
  XPlotGoogleChart(``Number of companies by industry``() |> Chart.Pie |> Chart.WithTitle("Pie graph of the number of companies per sector") |> Chart.WithLegend(true) )
  XPlotGoogleChart(``Number of companies by industry``() |> Chart.Bar |> Chart.WithTitle( "Bar graph of the number of companies per sector"))
  H2 "Data"
  Link("ASX Data summary","website\\ASXSummary.xls\"")
  H2 "Australian data file" ]
|> SimpleReport.toHtml
|> (fun h -> File.WriteAllText((sprintf "%s\\website\\ASXSummary.htm" __SOURCE_DIRECTORY__), h))                 
                                           