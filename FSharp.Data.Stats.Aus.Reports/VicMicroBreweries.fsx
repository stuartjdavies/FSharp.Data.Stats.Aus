#load "Setup.fsx"

open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open System.Net
open System
open Setup

type MicroBreweries = CsvProvider<"C:\\Users\\stuart\\Documents\\GitHub\\FSharp.Data.Stats.Aus\\FSharp.Data.Stats.Aus.Experiments\\Data\\MicroBreweries.csv">
type GeoJsonSchema = JsonProvider<"C:\\Users\\stuart\\Documents\\GitHub\\FSharp.Data.Stats.Aus\\FSharp.Data.Stats.Aus.Experiments\\Data\\GeoInfoSchema.json">

let getGoogleGeoCode address = sprintf "http://maps.googleapis.com/maps/api/geocode/json?address=%s&sensor=false" address;
let downloadPageAsString(url : string) = (new WebClient()).DownloadString url 
let getGeoInfo address =
        address |> sprintf "http://maps.googleapis.com/maps/api/geocode/json?address=%s&sensor=false"
                |> downloadPageAsString |> GeoJsonSchema.Parse    
let getGeoLatAndLng address =
        let r = address |> getGeoInfo 
        (double) r.Results.[0].Geometry.Location.Lat, (double) r.Results.[0].Geometry.Location.Lng
let items = MicroBreweries.Load("http://data.gov.au/storage/f/2013-05-12T212958/tmpDKnZGomicrobreweries.csv").Rows |> Seq.toArray

//printfn "List of Micro Breweries in Victoria"
//items |> Array.iteri(fun i item -> printfn "%d. %s" (i + 1) item.Name)
let locations = items |> Array.map(fun item -> item.Name, sprintf "%s %s VIC" item.Address item.``Town/Suburb``)
                      |> Array.map(fun (n, a) -> System.Threading.Thread.Sleep(200)
                                                 let lat, lng = getGeoLatAndLng a
                                                 lat, lng, n)
                      |> Chart.Map
                             
[ Title "Victorian Micro Breweries Data Analysis Report"
  TopHeader("Victorian Micro Beer breweries", "")                  
  H2 "Introduction"
  P "This is all the microbrewries in Victoria"
  H2 "Key Points"
  List [ sprintf "There are %d Micro Breweries in Victorian" (items.Length); "Most of the micro breweries are locating in Victoria" ]
  H2 "Locations"
  XPlotGoogleChart locations
  H2 "List of Microbrewries in Victoria"
  P "This is all the microbrewries in Victoria"        
  Table(["No"; "Name"; "Address"; "Town/Suburb"; "Postcode"; "Email"], items |> Seq.mapi(fun i item -> [ (i + 1).ToString(); item.Name; item.Address; item.``Town/Suburb``; item.Postcode.ToString(); item.Email ]) |> Seq.toList) ] 
|> SimpleReport.toHtml
|> (fun h -> File.WriteAllText((sprintf "%s\\website\\VicMicroBreweries.htm" __SOURCE_DIRECTORY__), h))                 
         
 