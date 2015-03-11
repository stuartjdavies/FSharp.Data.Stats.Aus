#load "packages/FsLab/FsLab.fsx"
#r @"..\packages\ExcelProvider.0.1.2\lib\net40\ExcelProvider.dll"
#r @"FSharp.Data.TypeProviders.dll" 

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

//
// Statistics of Highly skilled visas approved in 2014
//
let filePath = __SOURCE_DIRECTORY__ + @"\Data\FA140301378-3.xls"
type ApplicationsSchema = ExcelFile<"Data\FA140301378-3.xls", "2012-13!A8:F65536">

let applications = new ApplicationsSchema(filePath)

let applicationsApprovedByIndustry = applications.Data |> Seq.filter(fun r -> r.``Nomination Approved``.CompareTo("Y") = 0)
                                                       |> Seq.groupBy(fun r -> r.``Sponsor Industry (self identified)``)
                                                       |> Seq.map(fun (g, rows) -> (g, (rows |> Seq.length)))
                                                       |> Seq.sortBy(fun (g, l) -> -l)
                                                                                                        
let applicationsNotApprovedByIndustry = applications.Data |> Seq.filter(fun r -> r.``Nomination Approved``.CompareTo("N") = 0)
                                                          |> Seq.groupBy(fun r -> r.``Sponsor Industry (self identified)``)
                                                          |> Seq.map(fun (g, rows) -> (g, (rows |> Seq.length)))
                                                          |> Seq.sortBy(fun (g, l) -> -l)

let applicationsApprovedByDesc = applications.Data |> Seq.filter(fun r -> r.``Nomination Approved``.CompareTo("Y") = 0)
                                                   |> Seq.groupBy(fun r -> r.``ANZSCO Description``)
                                                   |> Seq.map(fun (g, rows) -> (g, (rows |> Seq.length)))
                                                   |> Seq.sortBy(fun (g, l) -> -l)

// Visa application results in 2013
applications.Data |> Seq.groupBy(fun r -> r.``Nomination Approved``)
                  |> Seq.map(fun (na, nas) -> let numberOfApplications = Seq.length(nas)
                                              let numberOfApplicationsPerDay = (float numberOfApplications) / 365.0
                                              let numberOfApplicationsPerHour = (float numberOfApplicationsPerDay) / 24.0
                                              (na, numberOfApplications, numberOfApplicationsPerDay, numberOfApplicationsPerHour))
                  |> Seq.iter (fun (na, numApp, numAppPerDay, numAppPerHour) ->  
                                       printfn "Application approved - %s, total - %d, per day - %.2f, per hour - %.2f" na numApp numAppPerDay numAppPerHour)

printfn "Total number of nominations %d in 2013" (Seq.length applications.Data) 

// Top 10 applications approved by Industry in 2013
applicationsApprovedByIndustry |> Seq.take 10 
                               |> Seq.iter (fun (g, l) -> printfn "Industry - %s, Number Approved - %d" g l)

// Top 10 Applications approved by Description in 2013
applicationsApprovedByDesc |> Seq.take 10 
                           |> Seq.iter (fun (g, l) -> printfn "Description - %s, Number Approved - %d" g l)


// 
// Migration Program By Outcome
//
let historicalMigrationStatsFileName = __SOURCE_DIRECTORY__ + @"\Data\historical-migration-stats.xls"
type MigrationProgramByOutcomeSchema = ExcelFile<"Data\historical-migration-stats.xls", "3.2!C14:G44">

let mg = new MigrationProgramByOutcomeSchema(historicalMigrationStatsFileName)

// Total migration
Chart.FastLine(([ for row in mg.Data -> row.Year, row.Total ]), Title="Total Migration")

// Migration by Stream
Chart.Combine([ Chart.Line(([ for row in mg.Data -> row.Year, row.``Family Stream``]), Name="Family Stream");
                Chart.Line(([ for row in mg.Data -> row.Year, row.``Skill Stream``]), Name="Skill Stream");
                Chart.Line(([ for row in mg.Data -> row.Year, row.``Special Eligibility``]), Name="Special Eligibility") ])
     .WithLegend(Enabled=true)
     .With3D()
     .WithTitle("Migration Programme outcome by stream, 1983–84 to 2012–13")
     .WithXAxis(Title="Years")
     .WithYAxis(Title="Migrants")

//
// Migration by Group
//
type MigrationProgramByGroupSchema = ExcelFile<"Data\historical-migration-stats.xls", "2.1!B14:T274">
 
let mgByGroup = new MigrationProgramByGroupSchema(historicalMigrationStatsFileName)

mgByGroup.Data 
|> Seq.filter(fun r -> let s = r.``Country Birth - major group``
                       String.IsNullOrEmpty(s) = false && s.ToString().Contains("total"))
|> Seq.map(fun r -> Chart.Line(["1996–97",r.``1996–97``;"1997–98",r.``1997–98``;"1998–99",r.``1998–99``;
                                              "1999–00",r.``1999–00``;"2000–01",r.``2000–01``;"2001–02",r.``2001–02``;
                                              "2002–03",r.``2002–03``;"2003–04",r.``2003–04``;"2004–05",r.``2004–05``;
                                              "2005–06",r.``2005–06``;"2006–07", r.``2006–07``;"2007–08",r.``2007–08``;
                                              "2008–09",r.``2008–09``;"2009–10",r.``2009–10``;"2010–11",r.``2010–11``;
                                              "2011–12", r.``2011–12``;"2012–13",r.``2012–13``], 
                                              Name=r.``Country Birth - major group``))
|> Chart.Combine
|> Chart.WithLegend(Enabled=true,InsideArea=false)

// Pie chart of group migration in 2013
mgByGroup.Data 
|> Seq.filter(fun r -> let s = r.``Country Birth - major group``
                       String.IsNullOrEmpty(s) = false && s.ToString().Contains("total"))
|> Seq.map(fun r -> (r.``Country Birth - major group``.Replace("total",String.Empty),r.``2012–13``))
|> Chart.Pie
|> Chart.WithLegend(Enabled=true)


