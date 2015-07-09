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
open FSharp.Data.Stats.Aus.TypeProviders

let downloadPageAsString(url : string)= (new WebClient()).DownloadString url 

let exchangeRates = new RBACsvStatisticsProvider<"""http://www.rba.gov.au/statistics/tables/csv/f11.1-data.csv""">()

let parseSingle (s : string) =
        let success, v = Single.TryParse(s) 
        if (success = true) then v else Single.NaN          
            
[ Chart.Line(exchangeRates.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.``A$1=USD``), Name="USD").WithLegend(Enabled=true) 
  Chart.Line(exchangeRates.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.``A$1=EUR``), Name="EUR")
  Chart.Line(exchangeRates.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.``A$1=GBP``), Name="GPS") ] |> Chart.Combine                                        

let commodityPrices = new RBACsvStatisticsProvider<"""http://www.rba.gov.au/statistics/tables/csv/i2-data.csv""">() 

[ Chart.Line(commodityPrices.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.``Commodity prices – A$``), Name="Commodity prices – A$").WithLegend(Enabled=true)   
  Chart.Line(commodityPrices.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.``Base metals prices – A$``), Name="Base metals prices – A$") ] |> Chart.Combine                                        

// There is a bug.
//let labourForce = new RBACsvStatisticsProvider<"http://www.rba.gov.au/statistics/tables/csv/h5-data.csv">()

//[ Chart.Line(labourForce.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.GLFSEPTSA), Name="Employment").WithLegend(Enabled=true)   
//  Chart.Line(labourForce.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.GLFSUPSA), Name="Unemployment") ] |> Chart.Combine                                        
//[ Chart.Line(labourForce.Data |> Seq.map(fun item -> DateTime.Parse(item.``Series ID``), parseSingle item.GLFSURSA), Name="Unemployment Rate").WithLegend(Enabled=true) ] |> Chart.Combine                                        

  
