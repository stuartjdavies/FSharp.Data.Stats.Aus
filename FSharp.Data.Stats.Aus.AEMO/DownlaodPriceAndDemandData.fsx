(*** hide ***)
#load "Setup.fsx"
(**
Download Price and Demand Electricity data from AEMO since records started
==========================================================================
*)
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

  
let downloadPageAsString(url : string)= (new WebClient()).DownloadString url         
                  
let getAnchorsOnPage url = let doc = new HtmlDocument()
                           downloadPageAsString url |> doc.LoadHtml 
                           doc.DocumentNode.Descendants().Where(fun x -> String.Compare(x.Name,"a", true) = 0)

let getYearsFromWebSite() : (AEMOYear * AEMOYearPageUrl) seq = 
                 getAnchorsOnPage """http://www.aemo.com.au/Electricity/Data/Price-and-Demand/Aggregated-Price-and-Demand-Data-Files"""
                 |> Seq.filter(fun anchor -> let success, num = Int16.TryParse(anchor.InnerText)
                                             success)
                 |> Seq.map(fun anchor -> (Int32.Parse(anchor.InnerText.Trim()), String.Format("http://www.aemo.com.au{0}", anchor.GetAttributeValue("href", null).Trim())))
                                           
let getMonthsFromWebSite (yearUrls : (AEMOYear * AEMOYearPageUrl) seq) : AEMOPriceAndDemandFileInfo seq = 
                    yearUrls |> Seq.map(fun (year, url) -> 
                                            getAnchorsOnPage url 
                                            |> Seq.filter(fun anchor -> anchor.Attributes.Contains("href") = true && 
                                                                        anchor.GetAttributeValue("href", null).Contains(".csv") && 
                                                                        anchor.GetAttributeValue("href", null).Contains((year.ToString())))
                                            |> Seq.map(fun anchor -> let href = anchor.GetAttributeValue("href", null).Trim()
                                                                     (year, getMonthFromFileName(anchor.GetAttributeValue("href", null).Trim()), 
                                                                      anchor.InnerText.Replace("&nbsp;", ""), href,
                                                                      href.Substring(href.LastIndexOf('/') + 1))))                                                                   
                             |> Seq.concat                            

// Can't figure out why the CsvProvider is not working
// type PriceAndDemandSchema = CsvProvider<"""./Data/DATA201411_VIC1.csv""">                                                                                                                                 
// let getPriceAndDemands (urls : string seq) = urls |> Seq.map(fun url -> use wc = new WebClient()
//                                                                        let csv = wc.DownloadString(url).Trim() 
//                                                                        PriceAndDemandSchema.Parse(csv).Rows)
let downloadFileAsString (url : string) = let wc = new WebClient()
                                          wc.DownloadString(url).Trim()
                                          
let getPriceAndDemandDataFileInfosFromWebSite() = getYearsFromWebSite() |> getMonthsFromWebSite

let downloadFile (src : string) (dest : string) = 
        //(new WebClient()).DownloadFile(src, dest)
        let wc = new WebClient()
        let s = wc.DownloadString(src).Trim()
        File.WriteAllText(dest, s)

let createIfNotExists directory =
        if Directory.Exists directory = false then
          Directory.CreateDirectory directory |> ignore          
        else
          ()               

let downloandPriceAndDemandFile (info : AEMOPriceAndDemandFileInfo) =
            let year, month, state, url, fileName = info
            
            [ sprintf "%s\Data" __SOURCE_DIRECTORY__
              sprintf "%s\Data\PriceAndDemand" __SOURCE_DIRECTORY__ 
              sprintf "%s\Data\PriceAndDemand\%d" __SOURCE_DIRECTORY__ year
              sprintf "%s\Data\PriceAndDemand\%d\%s" __SOURCE_DIRECTORY__ year state ]
            |> Seq.iter createIfNotExists
            
            let dest = (sprintf "%s\Data\PriceAndDemand\%d\%s\%s" __SOURCE_DIRECTORY__ year state fileName)
            downloadFile url dest
            printfn "Saved file %s to %s" url dest

let fileInfos = getPriceAndDemandDataFileInfosFromWebSite() |> Seq.toArray

fileInfos |> Array.Parallel.iter downloandPriceAndDemandFile

printfn "Saved %d price and demand data files" (fileInfos |> Seq.length)

let combindedCsvFileName = __SOURCE_DIRECTORY__ + "\Data\PriceAndDemandCombined.csv"

(** Get all records and save as a csv file **)
getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
|> Array.Parallel.map getPriceAndDemandsFromFileName
|> Array.concat
|> Frame.ofRecords
|> (fun df -> df.SaveCsv combindedCsvFileName)

printfn "All records have been put into csv file %s" combindedCsvFileName

(** Save records by state **)
getAllFileNames (__SOURCE_DIRECTORY__ + "\Data\PriceAndDemand")
|> Array.Parallel.map getAEMOPriceAndDemandFileInfoFromFile
|> Seq.groupBy(fun (_,_,state,_,_) -> state)
|> Seq.toArray
|> Array.Parallel.map (fun (state, fileInfos) -> fileInfos |> Seq.map (fun (_,_,_, filePath, _) -> filePath)
                                                           |> Seq.map getPriceAndDemandsFromFileName
                                                           |> Seq.concat
                                                           |> Frame.ofRecords
                                                           |> (fun df -> let stateCSV = __SOURCE_DIRECTORY__ + """\Data\""" + state + ".csv"
                                                                         df.SaveCsv stateCSV
                                                                         printfn "Saved file %s" stateCSV))