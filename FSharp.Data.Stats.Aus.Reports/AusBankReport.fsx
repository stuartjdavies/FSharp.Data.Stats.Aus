#load "Setup.fsx"

open System.Net
open Microsoft.FSharp.Control.WebExtensions
open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open Setup


let stocks = [ ("Westpac", "http://ichart.finance.yahoo.com/table.csv?s=WBC.AX"); 
               ("Commonwealth Bank", "http://ichart.finance.yahoo.com/table.csv?s=CBA.AX"); 
               ("ANZ", "http://ichart.finance.yahoo.com/table.csv?s=ANZ.AX");
               ("Bendigo And Adelaide Bank Ltd", "http://ichart.finance.yahoo.com/table.csv?s=BEN.AX");
               ("Macquarie Group Limited", "http://ichart.finance.yahoo.com/table.csv?s=MQG.AX")]

type StockSchema = CsvProvider<"http://ichart.finance.yahoo.com/table.csv?s=WBC.AX">               

let stockChart = seq { for s in stocks -> async { return (fst(s), 
                                                                  ([for row in StockSchema.Parse(Http.RequestString(snd(s))).Rows -> row.Date, row.Open]))}}
                         |> Async.Parallel
                         |> Async.RunSynchronously
                         |> (fun xs -> let f = xs |> Seq.map fst
                                       let s = xs |> Seq.map snd
                                       s |> Chart.Line |> Chart.WithLabels f)
                         

[ Title "Australian Bank Stock Prices"
  TopHeader("Australian Bank Stock Prices", "")                  
  H2 "Introduction"
  P "This report shows how Australian Bank Stock Prices have climbed in recent history"
  H2 "Key Points"
  List [ "Banks are making a lot of money" ]
  H2 "Graph of bank closing prices from 1988 to present"
  XPlotGoogleChart stockChart]
|> SimpleReport.toHtml
|> (fun h -> File.WriteAllText((sprintf "%s\\website\\AusBanks.htm" __SOURCE_DIRECTORY__), h))      



