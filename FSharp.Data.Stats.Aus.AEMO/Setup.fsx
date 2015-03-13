(*** hide ***)
#load "packages/FsLab/FsLab.fsx"
#I "../packages/HtmlAgilityPack.1.4.9/lib/Net45/"
#r "HtmlAgilityPack.dll"

(**
FsLab Experiment
================
*)

open Deedle
open System
open System.IO

// Can't figure out why the CsvProvider is not working
// type PriceAndDemandSchema = CsvProvider<"""./Data/DATA201411_VIC1.csv""">                                                                                                                                 
// let getPriceAndDemands (urls : string seq) = urls |> Seq.map(fun url -> use wc = new WebClient()
//                                                                        let csv = wc.DownloadString(url).Trim() 
//                                                                         PriceAndDemandSchema.Parse(csv).Rows)                                                                                                                     
// Using record implementation. Might end up better.

type AEMOYear = Int32
type AEMOMonth = Int32
type AEMOState = String
type AEMOYearPageUrl = String
type AEMOFileLocation = String
type AEMOShortFileName = String
type AEMOPriceAndDemandFileInfo = AEMOYear * AEMOMonth * AEMOState * AEMOFileLocation * AEMOShortFileName
    
type PriceAndDemandRecord = {
        Region : String;
        SettlementDate : DateTime;
        TotalDemand : float;
        RRP : float;
        PeriodType : string;   
     }


let getPriceAndDemandsFromFileName (fileName : string) =
                                        File.ReadAllLines(fileName) 
                                        |> Seq.skip 1
                                        |> Seq.toArray
                                        |> Array.Parallel.map(
                                                    fun row -> let fields = row.Split(',')
                                                               { PriceAndDemandRecord.Region=fields.[0];
                                                                                      SettlementDate=DateTime.Parse(fields.[1].Replace("\"", ""));
                                                                                      TotalDemand=(float fields.[2]);
                                                                                      RRP=(float fields.[3]);
                                                                                      PeriodType=fields.[4]; })

let rec getAllFileNames path =             
            let files = Directory.GetFiles path
            let filesInSubDir = Directory.GetDirectories path
                                |> Array.Parallel.map getAllFileNames 
                                |> Array.concat
            
            Array.append files filesInSubDir  

let getMonthFromFileName(url : string)  = try
                                            Int32.Parse(url.Substring(url.LastIndexOf("_") - 2, 2))
                                          with
                                          | ex -> 0 

let getAEMOPriceAndDemandFileInfoFromFile (filePath : string) : AEMOPriceAndDemandFileInfo =
        let fields = filePath.Substring((filePath.LastIndexOf("\PriceAndDemand") + "PriceAndDemand".Length + 2))
                             .Split('\\')
        (int fields.[0]), getMonthFromFileName(filePath), fields.[1], filePath, fields.[2]

