#load "Setup.fsx"

open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open System.Net
open System
open Setup

//TopHeader("List of Australian Statistics Reports", "")        

[ Title "List of Australian Statistics Reports"          
  H2 "List of Australian Statistics Reports"
  LinkList [ "Victorian Micro Breweries", "VicMicroBreweries.htm"                      
             "Australian Currency Exchange Rates", "RBAExchangeRates.htm"
             "World population", "WorldPopulation.htm" 
             "Australian Bank Stocks", "AusBanks.htm" ] ] 
|> SimpleReport.toHtml 
|> (fun h -> File.WriteAllText((sprintf "%s\\website\\index.htm" __SOURCE_DIRECTORY__), h))    
