#load "Setup.fsx"

open FSharp.Data
open XPlot.GoogleCharts
open System.IO
open System.Net

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

printfn "Number of Victorian Micro Breweries - %d" (items.Length)

printfn "List of Micro Breweries in Victoria"
items |> Array.iteri(fun i item -> printfn "%d. %s" (i + 1) item.Name)

let locations = items |> Array.map(fun item -> item.Name, sprintf "%s %s VIC" item.Address item.``Town/Suburb``)
                      |> Array.map(fun (n, a) -> System.Threading.Thread.Sleep(200)
                                                 let lat, lng = getGeoLatAndLng a
                                                 lat, lng, n)
    
locations |> Chart.Map |> Chart.WithOptions (Options(showTip = true))





