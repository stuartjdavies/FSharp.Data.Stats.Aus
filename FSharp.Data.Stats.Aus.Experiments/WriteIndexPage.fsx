#load "Setup.fsx"

open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open System.Net
open System
open Setup

Report [ Title "List of Australian Statistics Reports"
         TopHeader("List of Australian Statistics Reports", "")                  
         Subheading "Reports"
         LinkList [ "Victorian Micro Breweries", "VicMicroBreweries.htm"                      
                    "Australian Currency Exchange", "RBAExchangeRates.htm" ] ] 
|> DataAnalysisReport.create 
|> (fun h -> File.WriteAllText("c:\\temp\\index.htm", h))    

// Australian bank prices
// 
     
