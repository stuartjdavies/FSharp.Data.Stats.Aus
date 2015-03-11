#load "Setup.fsx"

open FSharp.Data.Stats.Aus.TypeProviders
open System.IO
open System
open FSharp.Charting

let filePath = __SOURCE_DIRECTORY__ + @"\data\Labour underutilisation by Age and Sex - Trend.xls"

let trends = new ABSExcelSchemaProvider<"""Data\Labour underutilisation by Age and Sex - Trend.xls""">(filePath)

let totalUnderUtiliRate = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  Total (Age) ;  Persons ; - (Trend)``)))
            
Chart.Line(totalUnderUtiliRate).WithTitle("Labour force underutilisation rate")
                      
let rate15_24 = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  15 - 24 ;  Persons ; - (Original)``)))
let rate25_34 = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  25 - 34 ;  Persons ; - (Original)``)))
let rate35_44 = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  35 - 44 ;  Persons ; - (Original)``)))
let rate45_54 = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  45 - 54 ;  Persons ; - (Original)``)))
let rate55_Over = trends.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), (Single.Parse(r.``Labour force underutilisation rate ;  55 and over ;  Persons ; - (Original)``)))

Chart.Combine([ Chart.Line(rate15_24, "Ages 15-24")
                Chart.Line(rate25_34, "Ages 25-34")
                Chart.Line(rate35_44, "Ages 35-44")
                Chart.Line(rate45_54, "Ages 45-54")
                Chart.Line(rate55_Over, "From 55 and over") ])
                .WithLegend(true)
                .WithTitle("Labour force underutilisation rate by age")            