#load "Setup.fsx"

open System.Net
open Microsoft.FSharp.Control.WebExtensions
open FSharp.Data
open FSharp.Charting

let stocks = [ ("Westpac", "http://ichart.finance.yahoo.com/table.csv?s=WBC.AX"); 
               ("Commonwealth Bank", "http://ichart.finance.yahoo.com/table.csv?s=CBA.AX"); 
               ("ANZ", "http://ichart.finance.yahoo.com/table.csv?s=ANZ.AX");
               ("Bendigo And Adelaide Bank Ltd", "http://ichart.finance.yahoo.com/table.csv?s=BEN.AX");
               ("Macquarie Group Limited", "http://ichart.finance.yahoo.com/table.csv?s=MQG.AX")]

type StockSchema = CsvProvider<"http://ichart.finance.yahoo.com/table.csv?s=WBC.AX">               

//
// Using Async
//
let asyncStopwatch = System.Diagnostics.Stopwatch.StartNew()
let stockDataWithAsync = seq { for s in stocks -> async { return (fst(s), 
                                                                  ([for row in StockSchema.Parse(Http.RequestString(snd(s))).Rows -> row.Date, row.Open]))}}
                         |> Async.Parallel
                         |> Async.RunSynchronously
                         |> Seq.map (fun (name, data) -> Chart.Line(data,Name=name).WithLegend(Enabled=true))
                         |> Chart.Combine
asyncStopwatch.Stop()

//
// Without Async 
//
let normalStopwatch = System.Diagnostics.Stopwatch.StartNew()                                              
let stockData = stocks |> Seq.map (fun (name,url) -> (name, Http.RequestString(url)))
                       |> Seq.map (fun (name, data) -> let stock = StockSchema.Parse(data)
                                                       (name, [ for row in stock.Rows -> row.Date, row.Open]))
                       |> Seq.map (fun (name, data) -> Chart.Line(data,Name=name).WithLegend(Enabled=true))
                       |> Chart.Combine
normalStopwatch.Stop()                                                                                              

let percentDiff(x : float, y : float) = (x / y) * 100.0 

printfn "Async Stopwatch - (%f ms)" asyncStopwatch.Elapsed.TotalMilliseconds
printfn "Normal Stopwatch - (%f ms)" normalStopwatch.Elapsed.TotalMilliseconds
printfn "Time Difference - %f ms" (normalStopwatch.Elapsed.TotalMilliseconds - asyncStopwatch.Elapsed.TotalMilliseconds)
printfn "Percent Difference - %f ms" (percentDiff(asyncStopwatch.Elapsed.TotalMilliseconds, normalStopwatch.Elapsed.TotalMilliseconds))

