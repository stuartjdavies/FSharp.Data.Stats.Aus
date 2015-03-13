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

let getTotalDemand (items : PriceAndDemandRecord seq) =
        items |> Seq.sumBy(fun item -> item.TotalDemand)

let getTotalDemandForEachRegion (rs : PriceAndDemandRecord seq) =      
        rs |> Seq.groupBy(fun r -> r.Region)
           |> Seq.map(fun (r, rs) -> r, getTotalDemand(rs))          
       
let getTotalDemandForEachRegionAndYear (rs : PriceAndDemandRecord seq) =      
        let sumByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, getTotalDemand xs) 
        
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, sumByYear items |> Seq.toArray)           

let getTotalDemandForEachRegionYearAndMonth (rs : PriceAndDemandRecord seq) =      
        let sumByMonth (items : PriceAndDemandRecord seq) =
               items |> Seq.groupBy(fun item -> item.SettlementDate.Month)
                     |> Seq.map(fun (m, xs) -> m, getTotalDemand xs) 
                     |> Seq.toArray
                       
        let groupByYear (items : PriceAndDemandRecord seq) =
                items |> Seq.groupBy(fun item -> item.SettlementDate.Year)
                      |> Seq.map(fun (y, xs) -> y, sumByMonth xs)
                      |> Seq.toArray
                                      
        rs |> Seq.groupBy(fun item -> item.Region)
           |> Seq.map(fun (r, items) -> r, groupByYear items)           
          
let rows = gePriceAndDemandRows()
let regions = getRegions rows |> Seq.toArray
let totalDemandForEachRegion = getTotalDemandForEachRegion rows |> Seq.toArray
let totalDemandForEachRegionAndYear = getTotalDemandForEachRegionAndYear rows |> Seq.toArray
let totalDemandForEachRegionYearAndMonth = getTotalDemandForEachRegionYearAndMonth rows |> Seq.toArray






                                                                                 