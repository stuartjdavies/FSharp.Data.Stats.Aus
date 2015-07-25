#load "../packages/FsLab.0.3.5/FsLab.fsx"
#r @"..\packages\ExcelProvider.0.1.2\lib\net40\ExcelProvider.dll"
#r @"..\FSharp.Data.Stats.Aus.TypeProviders\bin\Debug\FSharp.Data.Stats.Aus.TypeProviders.dll"
//#load "../packages/XPlot.GoogleCharts.1.1.6/XPlot.GoogleCharts.fsx"
#I "../packages/HtmlAgilityPack.1.4.9/lib/Net45/"
#r "HtmlAgilityPack.dll"
#r "PresentationCore.dll"
#r "PresentationFramework.dll"
#r "System.Xaml.dll"
#r "WindowsBase.dll"
#r @"FSharp.Data.TypeProviders.dll" 
#r "Microsoft.Office.Interop.Excel.dll"

open System
open XPlot.GoogleCharts
open System.IO

if Directory.Exists(__SOURCE_DIRECTORY__ + "\\Data") = false then
    Directory.CreateDirectory(__SOURCE_DIRECTORY__ + "\\Data") |> ignore

if Directory.Exists(__SOURCE_DIRECTORY__ + "\\Website") = false then
    Directory.CreateDirectory(__SOURCE_DIRECTORY__ + "\\Website") |> ignore

type SimpleReportItem =      
     | TopHeader of header : string * textbelow : string
     | H2 of string      
     | Table of headers : string list * data : string list list
     | List of string list
     | LinkList of (string * string) list
     | Link of href : string * text : string
     | XPlotGoogleChart of c : GoogleChart
     | Header of string
     | Title of string     
     | P of string     
     
 
module SimpleReport  =
            let joinLines (xs : seq<string>) = String.Join("\r", xs)
    
            let getStandardBootStrapThemeHtml() =  [ "<link href=\"starter-template.css\" rel=\"stylesheet\">"
                                                     "<link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css\">"
                                                     "<link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap-theme.min.css\">"
                                                     "<script src=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js\"></script>" ] 
                                                   |> joinLines            

            let renderItemList (items : string list) =
                        [ "<ul>"
                          items |> Seq.map(fun item -> sprintf "<li>%s</li>" item) |> joinLines
                          "</ul>" ] |> joinLines

            let renderLinkList (items : (string * string) list) =
                        [ "<ul>"
                          items |> Seq.map(fun (f,s) -> sprintf "<li><a href=\"%s\">%s</a></li>" s f) |> joinLines
                          "</ul>" ] |> joinLines

            let renderDataTable(headers : string list, dataRows : string list list) =
                    let rowHtml = dataRows |> Seq.map(fun r ->  [ "<tr>"
                                                                  r |> Seq.map(fun r -> sprintf "<td>%s</td>\r" r) |> (fun tds -> String.Join("\r", tds)) 
                                                                  "</tr>" ] |> (fun xs -> String.Join("\r", xs)))
                                           |> joinLines
                                                                           
                    let headerHtml = printfn "%d" (headers |> List.length)
                                     headers |> List.map(fun h -> sprintf "<th>%s</th>" h) 
                                             |> List.rev
                                             |> List.fold(fun acc h -> sprintf "%s%s" h acc ) ""

                    [ "<div class=\"col-md-6\">"
                      "<table class=\"table\">"
                      "<thead>"
                      "<tr>"
                      headerHtml
                      "</tr>"                       
                      "</thead>"
                      "<tbody>"
                      rowHtml
                      "</tbody>"
                      "</table>"
                      "</div>"] |> joinLines
                                       
            let renderHead (items : SimpleReportItem list) =                                       
                    let headerItems items = items |> List.map(fun item -> match item with
                                                                          | Title t -> sprintf "<title>%s</title>" t                    
                                                                          | _ -> "")
                                                  |> joinLines

                                        
                    [ "<head>"
                      "<script type=\"text/javascript\" src=\"https://www.google.com/jsapi\"></script>"
                      "<script type=\"text/javascript\">"
                      "google.load(\"visualization\", \"1.1\", { packages: [\"corechart\", \"annotationchart\", \"calendar\", \"gauge\", \"geochart\", \"map\", \"sankey\", \"table\", \"timeline\", \"treemap\"] })"
                      "</script>"
                      headerItems items
                      getStandardBootStrapThemeHtml()
                      "</head>" ] |> joinLines
                                        

            let renderHeader() =  
                    [ "<nav class=\"navbar navbar-inverse navbar-fixed-top\">" 
                      "<div class=\"container\">"
                      "<div class=\"navbar-header\">"
                      "<button type=\"button\" class=\"navbar-toggle collapsed\" data-toggle=\"collapse\" data-target=\"#navbar\" aria-expanded=\"false\" aria-controls=\"navbar\">"
                      " <span class=\"sr-only\">Toggle navigation</span>"
                      " <span class=\"icon-bar\"></span>"
                      " <span class=\"icon-bar\"></span>"
                      " <span class=\"icon-bar\"></span>"
                      "</button>"
                      "<a class=\"navbar-brand\" href=\"#\">Australian Statistics</a>"
                      "</div>"
                      "<div id=\"navbar\" class=\"collapse navbar-collapse\">"
                      "<ul class=\"nav navbar-nav\">"
                      "<li class=\"active\"><a href=\"index.htm\">Home</a></li>"
                      "<li><a href=\"#about\">About</a></li>"
                      "<li><a href=\"#contact\">Contact</a></li>"
                      "</ul>"
                      "</div><!--/.nav-collapse -->"
                      "</div>"
                      "</nav>" ] |> joinLines

            let renderTopHeader (h : string) (st : string) =
                      [ "<div class=\"jumbotron\">"
                        "<center>"
                        sprintf "  <h1>%s</h1>" h
                        sprintf "  <p class=\"lead\">%s</p>" st                                                     
                        "</center>"
                        "</div>" ] |> joinLines
                     
            let renderBody (items : SimpleReportItem list) =                                       
                    items |> List.fold(fun acc item -> match item with                                                                                                
                                                       | TopHeader(h,st) -> (renderTopHeader h st)::acc
                                                       | Header h -> (sprintf "<h1>%s</h1>\r" h)::acc                                       
                                                       | H2 h -> (sprintf "<h2>%s</h2>\r" h)::acc   
                                                       | P h -> (sprintf "<p>%s</p>\r" h)::acc  
                                                       | LinkList items -> (renderLinkList items)::acc
                                                       | List items -> (renderItemList items)::acc
                                                       | Table(hs, rs) -> renderDataTable(hs, rs)::acc                                                      
                                                       | XPlotGoogleChart c -> c.Html::acc                                                                                                           
                                                       | _ -> acc) List.empty                          
                          |> List.rev |> (fun xs -> [ "<body>"
                                                      renderHeader()
                                                      "<div class=\"container\">"
                                                      xs |> joinLines
                                                      "</div><!-- /.container -->"
                                                      "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\"></script>"
                                                      "<script src=\"../../dist/js/bootstrap.min.js\"></script>"
                                                      "<script src=\"../../assets/js/ie10-viewport-bug-workaround.js\"></script>"
                                                      "</body>"] |> joinLines) 
                                                                              
            let toHtml (items : SimpleReportItem list) =                     
                     [ "<html>"
                       renderHead items                                       
                       renderBody items                                        
                       "</html>" ] |> joinLines  




