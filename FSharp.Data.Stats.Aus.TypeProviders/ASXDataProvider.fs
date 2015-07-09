namespace FSharp.Data.Stats.Aus.TypeProviders

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open ProviderImplementation.ProvidedTypes
open System.Text.RegularExpressions
open System.IO
open Excel.Core
open Excel
open System.Net

type ASXCompany = { CompanyName : String; ASXCode : String; GICSIndustryGroup : String }
type StockItem = { ASXCode : String; Date : DateTime; Open : Single; High : Single; 
                   Low : Single; Close : Single; Volume : Single; AdjClose : Single }

module ASXDataService = 
            let downloadPageAsString(url : string)= (new WebClient()).DownloadString url   

            let getASXCompanies() = downloadPageAsString "http://www.asx.com.au/asx/research/ASXListedCompanies.csv" 
                                    |> (fun (s : string) -> s.Split('\n'))
                                    |> (fun lines -> lines.[3..])
                                    |> Array.filter(fun line -> line.Trim() <> String.Empty)
                                    |> Array.map(fun line -> let vs = line.Split(',')
                                                             { ASXCompany.CompanyName = vs.[0].Replace("\"","");
                                                               ASXCode =vs.[1]; GICSIndustryGroup=vs.[2].Replace("\"",""); })                                                                                       
                           
            let toStockItem asxCode (line: string) = line.Split(',')
                                                     |> (fun fields -> {StockItem.Date = DateTime.Parse(fields.[0]);
                                                                                  ASXCode = asxCode;
                                                                                  Open = Single.Parse(fields.[1]);
                                                                                  High = Single.Parse(fields.[2]);
                                                                                  Low = Single.Parse(fields.[3]);
                                                                                  Close = Single.Parse(fields.[4]);
                                                                                  Volume = Single.Parse(fields.[5]);
                                                                                  AdjClose = Single.Parse(fields.[6])})
                                               