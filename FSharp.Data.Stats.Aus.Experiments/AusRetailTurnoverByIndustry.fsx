#load "Setup.fsx"

open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic

open System.IO
open FSharp.ExcelProvider
open Microsoft.FSharp.Data.TypeProviders
open System.Linq
open System.Data
open FSharp.Charting


let filePath = __SOURCE_DIRECTORY__ + @"\Data\850101.xls" 
let excelFile = @"http://www.abs.gov.au/ausstats/meisubs.NSF/log?openagent&850101.xls&8501.0&Time%20Series%20Spreadsheet&9AD05A7541BD9AA3CA257D630017AAC5&0&Aug%202014&01.10.2014&Latest"
type RetailTurnoverFiguresSchema = ExcelFile<"""Data\850101.xls""", "Data1!A1:V399", true>
let retailTurnoverFigures = new RetailTurnoverFiguresSchema(filePath)

let r = retailTurnoverFigures.Data |> Seq.length

let retailFigures = retailTurnoverFigures.Data                 
                    |> Seq.skip(9)  
                    |> Seq.map(fun r -> (r.``Turnover ;  Total (State) ;  Cafes, restaurants and takeaway food services ;``.AsFloat(),                                
                                         r.``Turnover ;  Total (State) ;  Clothing, footwear and personal accessory retailing ;``.AsFloat(),
                                         r.``Turnover ;  Total (State) ;  Department stores ;``.AsFloat(),
                                         r.``Turnover ;  Total (State) ;  Food retailing ;``.AsFloat(),
                                         r.``Turnover ;  Total (State) ;  Household goods retailing ;``.AsFloat(),
                                         r.``Turnover ;  Total (State) ;  Other retailing ;``.AsFloat(),
                                         r.``Turnover ;  Total (State) ;  Total (Industry) ;``.AsFloat())) 
                                                   

type RetailTurnoverDateExcelSchema = ExcelFile<"""Data\850101.xls""", "Data1!A10:A399", true>
let retailTurnoverSeriesIds = new RetailTurnoverDateExcelSchema(filePath)
let seriesIdDates = retailTurnoverSeriesIds.Data |> Seq.map(fun r -> DateTime.FromOADate(r.``Series ID``.AsFloat()))
           
let retailTurnover = (seriesIdDates, retailFigures)
                     ||> Seq.map2(fun id (a,b,c,d,e,f,g)  -> (id,a,b,c,d,e,f,g)) 
                     |> Seq.cache

["Cafe, resturants takeaway food service", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, a ];
 "Clothing, footwear and personal accessory retailing", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, b ];
 "Department stores", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, c ];
 "Food retailing", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, d ];
 "Household goods retailing", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, e ];
 "Other retailing", [ for (id, a,b,c,d,e,f,g) in retailTurnover -> id, f ]] 
|> Seq.map (fun (name, data) -> Chart.Line(data,Name=name).WithLegend(Enabled=true))
|> Chart.Combine 

let getPercentage(num : float,total : float) = (num/total) * 100.00

retailTurnover 
|> Seq.last
|> (fun (id,a,b,c,d,e,f,g) -> (id, getPercentage(a,g), getPercentage(b,g), getPercentage(c,g),
                                   getPercentage(d,g),getPercentage(e,g), getPercentage(f,g)))                                                                                             
|> (fun (id,a,b,c,d,e,f) -> [sprintf "Cafe, resturants takeaway food service (%.2f)" a, a;
                             sprintf "Clothing, footwear and personal accessory retailing (%.2f)" b, b;
                             sprintf "Department stores (%.2f)" c, c;
                             sprintf "Food retailing (%.2f)" d, d;
                             sprintf "Household goods retailing (%.2f)" e, e;
                             sprintf "Other retailing (%.2f)" f, f])                                                            
|> Chart.Pie

                              

