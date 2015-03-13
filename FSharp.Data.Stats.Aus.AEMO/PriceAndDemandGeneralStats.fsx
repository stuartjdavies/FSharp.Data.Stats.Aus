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

let gePriceAndDemandRows() = 
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
        let sumByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, calc xs) 
        
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, sumByYear items |> Seq.toArray)           
           |> Seq.toArray

let runCalcForEachRegionYearAndMonth calc (rs : PriceAndDemandRecord seq) =      
        let sumByMonth (items : PriceAndDemandRecord seq) =
               items |> Seq.groupBy(fun item -> item.SettlementDate.Month)
                     |> Seq.map(fun (m, xs) -> m, calc xs) 
                     |> Seq.toArray
                       
        let groupByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, sumByMonth xs)
                      |> Seq.toArray
                                      
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, groupByYear items)
           |> Seq.toArray      
                    
let rows = gePriceAndDemandRows()
let regions = getRegions rows 
let totalDemandForEachRegion = rows |> runCalcForEachRegion getTotalDemand 
let totalDemandForEachRegionAndYear = rows |> runCalcForEachRegionAndYear getTotalDemand 
let totalDemandForEachRegionYearAndMonth = rows |> runCalcForEachRegionYearAndMonth getTotalDemand
let avgDemandForEachRegionYearAndMonth = rows |> runCalcForEachRegionYearAndMonth getTotalDemandAvg

// Windows





                                                                                 