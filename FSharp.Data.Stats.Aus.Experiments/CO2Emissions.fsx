#load "Setup.fsx"

open FSharp.Data
open RProvider
open RProvider.``base``
open Deedle
open Deedle.RPlugin
open RProvider.Internal.Converters
open RProvider.rworldmap 

let wb = WorldBankData.GetDataContext()
let countries = wb.Countries
 
let emissions2013 = series [ for c in countries -> c.Code => c.Indicators.``CO2 emissions (metric tons per capita)``.[2010]]

let df = frame [ "Emissions2013" => emissions2013; ]
df?Codes <- df.RowKeys

let map = R.joinCountryData2Map(df,"ISO3","Codes")
R.mapCountryData(map,"Emissions2013") 