#load "Setup.fsx"

open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic
open FSharp.Charting

open System.IO
open FSharp.ExcelProvider
open System.Linq
open System.Data

let filePath = __SOURCE_DIRECTORY__ + @"\Data\AusBeerConsuption.xls"
type LitresOfPureAlcoholSchema = ExcelFile<"Data\AusBeerConsuption.xls", "Table_1!B6:G75", true>
type PerCapitaConsumptionSchema = ExcelFile<"Data\AusBeerConsuption.xls", "Table_1!H6:N75", true>
type RawDatesSchema = ExcelFile<"Data\AusBeerConsuption.xls", "Table_1!A6:A75", true>
type CiderSchema = ExcelFile<"Data\AusBeerConsuption.xls", "Table_6!A6:C15", true>
let litresOfPureAlcohol = new LitresOfPureAlcoholSchema(filePath)
let perCapitaConsumption = new PerCapitaConsumptionSchema(filePath)
let rawDates = new RawDatesSchema(filePath)

let getYearAsInt(s : string) = s.Substring(0, s.IndexOf("-")) 
let dates = rawDates.Data |> Seq.map(fun s -> s.GetValue(0).ToString() |> getYearAsInt)

//
// Total Consumption
//

// Graph of Total litres of consumption
(dates, litresOfPureAlcohol.Data) ||> Seq.map2(fun d r -> (d,r.Total)) |> Chart.FastLine 

// Graph of total per capita consumption
(dates, perCapitaConsumption.Data) ||> Seq.map2(fun d r -> (d,r.Total)) |> Chart.FastLine 

// After 2000
let alcoholConsumedAfter2000 = (dates, litresOfPureAlcohol.Data) ||> Seq.map2(fun d r -> (Int32.Parse(d),r.``Low strength(d)``, r.``Mid strength(e)``,r.GetValue(4)))                                                                       
                                                                  |> Seq.filter(fun (d,_,_,_) -> d >= 2000)
                                                                                                                                
//
// Cider analysis 
//
let ciderData = new CiderSchema(filePath)

// Litres of pure alcohol 
ciderData.Data |> Seq.map(fun r -> (getYearAsInt(r.GetValue(0).ToString()), r.``    Litres of pure alcohol ('000 litres)``))
               |> Chart.FastLine

// Litres per capita consumption
ciderData.Data |> Seq.map(fun r -> (getYearAsInt(r.GetValue(0).ToString()), r.``Per capita consumption (litres)(a) ``))
               |> Chart.FastLine