#load "Setup.fsx" 

open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic

open System.IO
open FSharp.ExcelProvider
open Microsoft.FSharp.Data.TypeProviders
open System.Linq
open System.Data

let filePath = __SOURCE_DIRECTORY__ + @"\Data\NGERS-2009-10.xlsx"
type EmissionsExcelSchema = ExcelFile<"""Data\NGERS-2009-10.xlsx""">
let emissions = new EmissionsExcelSchema(filePath)

let rows = emissions.Data                 
           |>  Seq.map(fun r -> 
                        try
                            (r.``Registered Corporations``,
                             r.``Total scope 1 greenhouse gas emissions (t CO2-e) ``,
                             r.``Total scope 2 greenhouse gas emissions (t CO2-e) ``,
                             r.``Total energy consumption (GJ)``)
                        with                           
                           | _ -> (String.Empty,0.0,0.0,0.0))                              
           |> Seq.filter(fun (d,_,_,_) -> d <> String.Empty)
           |> Seq.toList            
                                                                                                                                                                  
let e1Total = rows |> Seq.sumBy(fun (d,e1,e2,ec) -> e1)
let e2Total = rows |> Seq.sumBy(fun (d,e1,e2,ec) -> e2)
let e3Total = rows |> Seq.sumBy(fun (d,e1,e2,ec) -> ec) 

let topPolluters = rows |> List.sortBy(fun (_,_,_,ec) -> ec)
                        |> List.rev
                        |> Seq.ofList
                        |> Seq.take 5                         

let xs = topPolluters |> Seq.map(fun (_,_,_,ec) -> Convert.ToDouble(ec))
let ys = topPolluters |> Seq.map(fun (d,_,_,_) -> d)

R.par(namedParams [
            "mar", box [7;6;4;2];
            "las", box 1])

R.barplot(namedParams [ 
            "height", box xs;
            "beside", box true;
            "legend", box ys;
            "main", box "Total energy consumption (GJ)";                  
            "col", box ["darkblue";"green";"red";"orange";"grey"]])
            
R.title(xlab="Registered Corporations", line=2) 
R.title(ylab="GJ", line=4)                  
