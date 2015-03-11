#load "Setup.fsx"

open FSharp.Data.Stats.Aus.TypeProviders
open System.IO
open System
open FSharp.Charting

let filePath = __SOURCE_DIRECTORY__ + @"\data\Labour force status by Sex - Trend.xls"

let trends = new ABSExcelSchemaProvider<"""data\Labour force status by Sex - Trend.xls""">(filePath)

let femaleFullTimeEmp = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Employed - full-time ;  Females ; - (Trend)``) * 1000.0F))
let maleFullTimeEmp = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Employed - full-time ;  Males ; - (Trend)``) * 1000.0F))
            

Chart.Combine(
            [ Chart.Line(maleFullTimeEmp, Name="Males in full time employment")
              Chart.Line(femaleFullTimeEmp, Name="Females in full time employment") ])
.WithLegend(true)
