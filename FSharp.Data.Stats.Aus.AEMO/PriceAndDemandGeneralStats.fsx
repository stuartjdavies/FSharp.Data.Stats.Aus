(*** hide ***)
#load "Setup.fsx"
(**
FsLab Experiment
================
*)

open FSharp.Data
open System.IO
open System
open Deedle
open Setup
open FSharp.Charting

let getPriceAndDemandBy f = 
                getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
                |> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
                |> Array.filter f 
                |> Array.Parallel.map(fun (_,_,_, filePath,_) -> getPriceAndDemandsFromFileName filePath)
                |> Array.concat
                            
let getStates() : AEMOState array = 
            getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
            |> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
            |> Array.Parallel.map (fun (_, _,state,_, _) -> state)
            |> Seq.distinct
            |> Seq.toArray   

(** Total Demand for the entire history of state **)
getStates() 
|> Array.Parallel.map(fun state -> let total =  getPriceAndDemandBy (fun (_,_,s,_,_) -> s=state) 
                                                |> Array.sumBy (fun item -> item.TotalDemand)
                                   state, total)
|> Chart.Bar

(** Graph of monthly demands by state **)
getStates()  
|> Array.Parallel.map(fun state -> state, (getPriceAndDemandBy (fun (_,_,s,_,_) -> s=state.ToString()) 
                                           |> Array.map(fun item -> item.SettlementDate, item.TotalDemand)))
|> Array.Parallel.map(fun (state, items) -> Chart.FastLine(items, Name=state)) 
|> Chart.Combine               

(** Total Demand since 1998 **)
getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
|> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
|> Seq.groupBy(fun (year,_,state,_,_) -> year, state)
|> Seq.map(fun ((year,state), fs) -> 
         let total = fs |> Seq.map(fun (_,_,_,fileName,_) -> getPriceAndDemandsFromFileName fileName) 
                        |> Seq.concat
                        |> Seq.sumBy(fun item -> item.TotalDemand)                        
         year, state, total)
|> Seq.groupBy(fun (_, state, _) -> state)
|> Seq.map(fun (state, items) -> let data = items |> Seq.map(fun (y,_,t) -> y, t) 
                                 Chart.Line(data, Name=state))
|> Chart.Combine |> Chart.WithLegend(true)

(** Average Price per year since 1998 **)                     
getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
|> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
|> Seq.groupBy(fun (year,_,state,_,_) -> year, state)
|> Seq.map(fun ((year,state), fs) -> 
         let total = fs |> Seq.map(fun (_,_,_,fileName,_) -> getPriceAndDemandsFromFileName fileName)
                        |> Seq.concat                       
                        |> Seq.averageBy(fun item -> item.RRP)                        
         year, state, total)
|> Seq.groupBy(fun (_, state, _) -> state)
|> Seq.map(fun (state, items) -> let data = items |> Seq.map(fun (y,_,t) -> y, t) 
                                 Chart.Line(data, Name=state))
|> Chart.Combine |> Chart.WithLegend(true)

(** Get the standard deviation **) 

//open MathNet
//open MathNet.Numerics.Statistics
//
//Chart.Combine [
//        Chart.Point([1, 1; 2, 4; 3,4],MarkerSize=20)
//        Chart.Line([1,1;2,2;3,3]) ]
//
//let s = [1,2,3]
//
//type Calculation<'a> = PriceAndDemandRecord array -> 'a
//
//let runCalc (calc : Calculation<'a>) (kvps : Map<string, (unit -> PriceAndDemandRecord array)>)  = 
//        kvps |> Map.toSeq
//             |> Seq.map(fun (s, getData) -> let data = getData()
//                                            s, calc(data))
//             |> Map.ofSeq
//                
//
//let getMinTotalDemand (items : PriceAndDemandRecord array) = items |> Seq.minBy(fun item -> item.TotalDemand)
//let getMaxTotalDemand (items : PriceAndDemandRecord array) = items |> Seq.minBy(fun item -> item.TotalDemand)
//let getAverageTotalDemand (items : PriceAndDemandRecord array) = items |> Seq.averageBy(fun item -> item.TotalDemand)
//let getAverageTotalDemand (items : PriceAndDemandRecord array) = items |> Seq.averageBy(fun item -> item.TotalDemand)


//let stats = new DescriptiveStatistics([0.0;2.0]);; 
//stats.Variance                  
//stats.



                                                                                 