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

let getPriceAndDemandRows() = 
                getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
                |> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
                |> Array.Parallel.map(fun (_,_,_, filePath,_) -> getPriceAndDemandsFromFileName filePath)
                |> Array.concat
                |> Array.sortBy(fun item -> item.SettlementDate)

let getRegions (items : PriceAndDemandRecord seq) =
                items |> Seq.map(fun item -> item.Region)
                      |> Seq.distinct
                      |> Seq.toArray

let getTotalDemand (items : PriceAndDemandRecord seq) =
        items |> Seq.sumBy(fun item -> item.TotalDemand)

let getTotalDemandMin (items : PriceAndDemandRecord seq) =
        items |> Seq.minBy(fun item -> item.TotalDemand)

let getTotalDemandMax (items : PriceAndDemandRecord seq) =
        items |> Seq.maxBy(fun item -> item.TotalDemand)

let getTotalDemandAvg (items : PriceAndDemandRecord seq) =
        items |> Seq.averageBy(fun item -> item.TotalDemand)

let runCalcForEachRegion calc (rs : PriceAndDemandRecord seq) =      
        rs |> Seq.groupBy(fun r -> r.Region)
           |> Seq.map(fun (r, rs) -> r, calc rs)          
           |> Seq.toArray

let runCalcForEachRegionAndYear calc (rs : PriceAndDemandRecord seq) =      
        let runCalcByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, calc xs) 
        
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, runCalcByYear items |> Seq.toArray)           
           |> Seq.toArray

let runCalcForEachRegionYearAndMonth calc (rs : PriceAndDemandRecord seq) =      
        let runCalcByMonth (items : PriceAndDemandRecord seq) =
               items |> Seq.groupBy(fun item -> item.SettlementDate.Month)
                     |> Seq.map(fun (m, xs) -> m, calc xs) 
                     |> Seq.toArray
                       
        let groupByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, runCalcByMonth xs)
                      |> Seq.toArray
                                      
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, groupByYear items)
           |> Seq.toArray      
                    
let rows = getPriceAndDemandRows()
let regions = getRegions rows 
let totalDemandForEachRegion = rows |> runCalcForEachRegion getTotalDemand 
let totalDemandForEachRegionAndYear = rows |> runCalcForEachRegionAndYear getTotalDemand 
let totalDemandForEachRegionYearAndMonth = rows |> runCalcForEachRegionYearAndMonth getTotalDemand
let avgDemandForEachRegionYearAndMonth = rows |> runCalcForEachRegionYearAndMonth getTotalDemandAvg

let getDemandDiff (items : PriceAndDemandRecord seq) =
             let h = Seq.head items
             let t = Seq.last items
             h, t, h.TotalDemand - t.TotalDemand

let ps = rows |> Seq.pairwise 
              |> Seq.toArray
              |> Array.Parallel.map(fun (f, s) -> f, s, f.TotalDemand - s.TotalDemand)
              |> Array.sortBy(fun (_, _, diff) -> -diff)

(** 1. The largest changes in electricity per 1/2 hour **)
ps |> Seq.take 10
   |> Seq.iteri(fun i (h, t, diff) -> printfn "%d (%s - %s) - %f" i (h.SettlementDate.ToString()) (t.SettlementDate.ToString()) diff)
         
rows |> Seq.iter(fun r -> printfn "%s - %f" (r.SettlementDate.ToString()) r.TotalDemand)         

(** 2. The largest change in electricity per 1 hour window **)
let rs = rows |> Seq.filter(fun r -> r.Region="NSW1")
              |> Seq.windowed(24*2*7)     
              |> Seq.map getDemandDiff    
              |> Seq.toArray
              |> Array.sortBy(fun (_,_,diff) -> -diff)
              |> Seq.take 10     
              |> Seq.iteri(fun i (h, t, diff) -> printfn "%d (%s - %s) - %f" i (h.SettlementDate.ToString()) (t.SettlementDate.ToString()) diff)

(** 3. The largest change in electricity per 1 day window **)            
(** 4. The largest change in electricity per 7 day window **)
(*let rs = rows |> Seq.filter(fun r -> r.Region="NSW1")
              |> Seq.windowed(24*2*7)     
              |> Seq.map getDemandDiff    
              |> Seq.toArray
              |> Array.sortBy(fun (_,_,diff) -> -diff)
              |> Seq.take 10     
              |> Seq.iteri(fun i (h, t, diff) -> printfn "%d (%s - %s) - %f" i (h.SettlementDate.ToString()) (t.SettlementDate.ToString()) diff)*)  

(** 3. The largest change in electricity per 30 day window **)

(** 4. **)

// Need to use Deedle 
// Windows





                                                                                 