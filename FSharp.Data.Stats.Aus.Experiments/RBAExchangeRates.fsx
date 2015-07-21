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
open System.Text.RegularExpressions
open HtmlAgilityPack
open Setup
open Deedle
open System.Diagnostics
open Microsoft.FSharp.Linq
open FSharp.Data.Stats.Aus.TypeProviders
open XPlot.GoogleCharts
 
let exchangeRates = new RBACsvStatisticsProvider<"""http://www.rba.gov.au/statistics/tables/csv/f11.1-data.csv""">()

let us = exchangeRates.Data |> Seq.toArray |> Array.map (fun item -> item.``Series ID``, item.``A$1=USD``)
let eur = exchangeRates.Data |> Seq.toArray |> Array.map (fun item -> item.``Series ID``, item.``A$1=EUR``)
let gps = exchangeRates.Data |> Seq.toArray |> Array.map (fun item -> item.``Series ID``, item.``A$1=GBP``)

let commodityPrices = new RBACsvStatisticsProvider<"""http://www.rba.gov.au/statistics/tables/csv/i2-data.csv""">() 

let commodityPricesAus = commodityPrices.Data |> Seq.toArray |> Seq.map(fun item -> item.``Series ID``, item.``Commodity prices – A$``)
let baseMetalPricesAus = commodityPrices.Data |> Seq.toArray |>  Seq.map(fun item -> item.``Series ID``, item.``Base metals prices – A$``)

let rs = [us; eur; gps]
         |> Chart.Line
         |> Chart.WithOptions  (Options ( curveType = "function", 
                                          legend = Legend(position = "bottom")))
         |> Chart.WithLabels ["USD"; "EUR"; "GDP"] |> (fun c -> c.InlineHtml)

let ps = [commodityPricesAus; baseMetalPricesAus]
         |> Chart.Line
         |> Chart.WithOptions  (Options (curveType = "function", 
                                              legend = Legend(position = "bottom") ))
         |> Chart.WithLabels ["Commodity prices in Aus$"; "Base metals prices in A$"] |> (fun p -> p.InlineHtml)

Report [ Title "Australian Exchange Rates"
         TopHeader("Australian Exchange Rates", "")                  
         Subheading "Introduction"
         Paragraph "Summary of Australian exchange rates"
         Subheading "Key Points"
         ItemList [ "This year the dollar has been decreasing"; "The australian dollar is still high" ]
         Subheading "Exchange Rates - Daily - 2014 to Current - F11.1"
         RGoogleChart rs
         Subheading "Commodity & metal prices "
         RGoogleChart ps ]
|> DataAnalysisReport.create 
|> (fun h -> File.WriteAllText("c:\\temp\\RBAExchangeRates.htm", h))                 
