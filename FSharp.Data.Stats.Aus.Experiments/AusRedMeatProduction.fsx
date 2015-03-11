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
open Microsoft.FSharp.Data.TypeProviders
open System.Linq
open System.Data
open Deedle


let filePath = __SOURCE_DIRECTORY__ + @"\Data\RedMeatProduced.xls"
type MeatProdSchema = ExcelFile<"Data\RedMeatProduced.xls", "Data1!A1:J517", true>

let mp = new MeatProdSchema(filePath)

let totalMeatProd = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Total (State) ;``.AsFloat() )
let totalMeatProdNsw = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  New South Wales ;``.AsFloat() )
let totalMeatProdAct = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Australian Capital Territory ;``.AsFloat() )
let totalMeatProdVic = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Victoria ;``.AsFloat() )
let totalMeatProdWa = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Western Australia ;``.AsFloat() )
let totalMeatProdQld = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Queensland ;``.AsFloat() )
let totalMeatProdNt = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Northern Territory ;``.AsFloat() )
let totalMeatProdTas = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> r.``Meat Produced ;  Total Red Meat ;  Tasmania ;``.AsFloat() )

let dates = mp.Data |> Seq.skip 9 |> Seq.map(fun r -> DateTime.FromOADate(Double.Parse(r.GetValue(0).ToString()))) 

Seq.zip dates totalMeatProd |> Chart.Line

[ ("NSW", (Seq.zip dates totalMeatProdNsw)); ("VIC", (Seq.zip dates totalMeatProdVic)); ("ACT", (Seq.zip dates totalMeatProdAct)); 
  ("WA", (Seq.zip dates totalMeatProdWa)); ("QLD", (Seq.zip dates totalMeatProdQld));("NT", (Seq.zip dates totalMeatProdNt)); 
  ("Tas", (Seq.zip dates totalMeatProdTas)); ]  
|> Seq.map (fun (name, data) -> Chart.Line(data,Name=name).WithLegend(Enabled=true))
|> Chart.Combine
|> Chart.WithTitle(Text="Total Red Meat produced by state (tonnes)")


let total = Series(dates,totalMeatProd)
let nsw = Series(dates,totalMeatProdNsw)
let act = Series(dates,totalMeatProdAct)
let vic = Series(dates,totalMeatProdVic)
let wa = Series(dates,totalMeatProdWa)
let qld = Series(dates,totalMeatProdQld)
let nt = Series(dates,totalMeatProdNt)
let tas = Series(dates,totalMeatProdTas)

total |> Stats.count
total |> Stats.max
total |> Stats.min
