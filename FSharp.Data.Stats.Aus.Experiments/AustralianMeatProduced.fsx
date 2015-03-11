#load "Setup.fsx"

open FSharp.Data.Stats.Aus.TypeProviders
open System.IO
open System
open FSharp.Charting

let filePath = __SOURCE_DIRECTORY__ + @"\Data\RedMeatProduced.xls"

let redMeatProd = new ABSExcelSchemaProvider<"""Data\RedMeatProduced.xls""">(filePath)

let totals = redMeatProd.Data |> Seq.map(fun r -> DateTime.Parse(r.Date), Int32.Parse(r.``Meat Produced ;  Total Red Meat ;  Total (State) ; - (Original)``))
 
Chart.Line(totals, Name="Total red meat produced in tones")
     .WithXAxis(Title="Year")
     .WithYAxis(Title="Produced")
     .WithTitle("Total red meat produced in tones")

let lastYear = redMeatProd.Data |> Seq.last

let lastYearMeatProd = [ "New south wales", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  New South Wales ; - (Original)``)
                         "Queensland", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  Queensland ; - (Original)``)
                         "South Australia", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  South Australia ; - (Original)``)
                         "Western Australia", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  Western Australia ; - (Original)``)
                         "Australian Capital Territory", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  Australian Capital Territory ; - (Original)``)
                         "Tasmania", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  Tasmania ; - (Original)``)
                         "Victoria", Int32.Parse(lastYear.``Meat Produced ;  Total Red Meat ;  Victoria ; - (Original)``) ] 

let pieChartTitle=String.Format("Meat Production in {0} in tonnes ", lastYear.Date)

Chart.Pie(lastYearMeatProd).WithLegend(true)


                
