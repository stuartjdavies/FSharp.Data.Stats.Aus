#load "Setup.fsx"
 
open System
open System.IO
open System.Linq
open System.Collections.Generic
open System.Net
open FSharp.Data
open FSharp.Charting
open System.Text.RegularExpressions
open HtmlAgilityPack



let downloadPageAsString(url : string)= (new WebClient()).DownloadString url         
                  
let getAnchorsOnPage url = let doc = new HtmlDocument()
                           downloadPageAsString url |> doc.LoadHtml 
                           doc.DocumentNode.Descendants().Where(fun x -> String.Compare(x.Name,"a", true) = 0)

let getYears() = getAnchorsOnPage """http://www.aemo.com.au/Electricity/Data/Price-and-Demand/Aggregated-Price-and-Demand-Data-Files"""
                 |> Seq.filter(fun anchor -> let success, num = Int16.TryParse(anchor.InnerText)
                                             success)
                 |> Seq.map(fun anchor -> (Int32.Parse(anchor.InnerText.Trim()), String.Format("http://www.aemo.com.au{0}", anchor.GetAttributeValue("href", null).Trim())))

let getMonthFromFileName(url : string)  = try
                                            Int32.Parse(url.Substring(url.LastIndexOf("_") - 2, 2))
                                          with
                                          | ex -> 0 
                                           
let getMonths (yearUrls : (int * string) seq) = 
                            yearUrls |> Seq.map(fun (year, url) -> getAnchorsOnPage url 
                                                                   |> Seq.filter(fun anchor -> anchor.Attributes.Contains("href") = true && 
                                                                                                  anchor.GetAttributeValue("href", null).Contains(".csv") && 
                                                                                                  anchor.GetAttributeValue("href", null).Contains((year.ToString())))
                                                                   |> Seq.map(fun anchor -> (year, getMonthFromFileName(anchor.GetAttributeValue("href", null).Trim()), 
                                                                                                    anchor.InnerText, anchor.GetAttributeValue("href", null).Trim())))                                                                   
                                      |> Seq.concat                            

let downloadFileAsString (url : string) = let wc = new WebClient()
                                          wc.DownloadString(url).Trim()
                                          
let getPricesAndDemandsFromString (s : string) = s.Split '\r' |> Seq.map(fun ln -> let fields = ln.Split(',')
                                                                                   (fields.[0], fields.[1], fields.[2], fields.[3], fields.[4]))
                                                                                                                          
                                                                                   
let getPriceAndDemandsFromUrls (urls : string seq) = seq {
                                                       for url in urls do
                                                         let s = downloadFileAsString url 
                                                         yield! getPricesAndDemandsFromString s
                                                     }               
                                        
let sortMonthsDesc months = months |> Seq.sortBy (fun (y, m, _, _) -> -((y * 10) + m))
let sortMonthsAsc months = months |> Seq.sortBy (fun (y, m, _, _) -> ((y * 10) + m))
let filterByState state months = months |> Seq.filter (fun (_, _, s, _) -> s = state )

getYears() |> getMonths |> Seq.length 


#time
getYears() |> getMonths |> sortMonthsDesc |> filterByState "NSW" |> Seq.take 24            
           |> Seq.map(fun (_, _, _, url) -> url) 
           |> getPriceAndDemandsFromUrls                     
           |> Seq.map(fun (_, dt, demand, rrp, _) -> (dt, rrp))             
           |> Seq.toList |> List.rev
           |> Chart.Line    
#time

