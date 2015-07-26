#load "Setup.fsx"

open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open System.Net
open System
open Setup

let wb = WorldBankData.GetDataContext()
let countries = wb.Countries

let worldPopulationGrowth =  [|1998..2013|] |> Array.map(fun year -> year.ToString(), [ for c in countries -> c.Indicators.``Population, total``.[year]] |> List.sum) 
                                            |> Chart.Line |> Chart.WithTitle("World population growth") 
let pop = [ for c in countries -> c.Name, c.Indicators.``Population, total``.[2013]] 
          |> Chart.Geo |> Chart.WithTitle("World map of population in 2013") |>  Chart.WithLabels(["Name"; "Population"])
let totalPopulation2013 = [ for c in countries -> c.Indicators.``Population, total``.[2013]] |> Seq.sum

[ Title "World Population"
  H1 "World Population"               
  H2 "Introduction"
  P "This report shows how the world population is distributed around the world"
  H2 "Key Points"
  List [ sprintf "In 2013 the world population was %.0f" totalPopulation2013 ]
  H2 "Graphs"
  XPlotGoogleChart pop   
  XPlotGoogleChart worldPopulationGrowth ]
|> SimpleReport.toHtml
|> (fun h -> File.WriteAllText((sprintf "%s\\website\\WorldPopulation.htm" __SOURCE_DIRECTORY__), h))                         