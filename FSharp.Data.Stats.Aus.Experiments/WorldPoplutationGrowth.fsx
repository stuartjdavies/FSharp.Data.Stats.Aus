#load "Setup.fsx"

// This example is lifted from 
// http://www.clear-lines.com/blog/post/Create-maps-using-R-Deedle-and-FSharp-type-providers.aspx
// I will create another example with this.

open FSharp.Data
open RProvider
open RProvider.``base``
open Deedle
open Deedle.RPlugin
open RProvider.Internal.Converters
 
let wb = WorldBankData.GetDataContext()
let countries = wb.Countries
 
let pop2000 = series [ for c in countries -> c.Code => c.Indicators.``Population, total``.[2000]]
let pop2010 = series [ for c in countries -> c.Code => c.Indicators.``Population, total``.[2010]]
let surface = series [ for c in countries -> c.Code => c.Indicators.``Surface area (sq. km)``.[2010]]
 
let df = frame [ "Pop2000" => pop2000; "Pop2010" => pop2010; "Surface" => surface ]
df?Codes <- df.RowKeys
 
open RProvider.rworldmap 

let map = R.joinCountryData2Map(df,"ISO3","Codes")
R.mapCountryData(map,"Pop2000") 

df?Density <- df?Pop2010 / df?Surface
df?Growth <- (df?Pop2010 - df?Pop2000) / df?Pop2000
 
let map2 = R.joinCountryData2Map(df,"ISO3","Codes")
R.mapCountryData(map2,"Density")
R.mapCountryData(map2,"Growth")
