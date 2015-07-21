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

type ReportItem =      
     | SimpleSection of header : string * paragraph : string 
     | TopHeader of header : string * textbelow : string
     | Subheading of string 
     | RawHtml of string
     | DataTable of headers : string list * data : string list list
     | ItemList of string list
     | LinkList of (string * string) list
     | GoogleMap of string
     | RGoogleChart of string
     | Header of string
     | Title of string     
     | Paragraph of string     
     | DataList of string array

type Report = | Report of ReportItem list
 
module DataAnalysisReport  =
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
                                       
            let renderHead (items : ReportItem list) =                                       
                    let headerItems = items |> List.fold(fun acc item -> match item with                                                                                                
                                                                         | Title h -> (sprintf "<title>%s</title>" h)::acc                                       
                                                                         | GoogleMap h -> [ "<script type=\"text/javascript\" src=\"https://www.google.com/jsapi\"></script>"
                                                                                            "<script type=\"text/javascript\">google.load(\"visualization\", \"1\", {packages:[\"map\"]})</script>" ]
                                                                                          |> (fun xs -> String.Join("\r", xs)::acc)                                                                                                                           
                                                                         | RGoogleChart h -> [ "<script type=\"text/javascript\" src=\"https://www.google.com/jsapi\"></script>"
                                                                                               "<script type=\"text/javascript\">google.load(\"visualization\", \"1\", {packages:[\"corechart\"]})</script>" ]
                                                                                            |> (fun xs -> String.Join("\r", xs)::acc)                                                                           
                                                                         | _ -> acc) List.empty                          
                                            |> List.rev |> joinLines
                                        
                    [ "<head>"
                      headerItems
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
                     
            let renderBody (items : ReportItem list) =                                       
                    items |> List.fold(fun acc item -> match item with                                                                                                
                                                       | TopHeader(h,st) -> (renderTopHeader h st)::acc
                                                       | Header h -> (sprintf "<h1>%s</h1>\r" h)::acc                                       
                                                       | Subheading h -> (sprintf "<h2>%s</h2>\r" h)::acc   
                                                       | Paragraph h -> (sprintf "<p>%s</p>\r" h)::acc  
                                                       | LinkList items -> (renderLinkList items)::acc
                                                       | ItemList items -> (renderItemList items)::acc
                                                       | RawHtml h -> h::acc
                                                       | DataTable(hs, rs) -> renderDataTable(hs, rs)::acc
                                                       | GoogleMap h -> h::acc
                                                       | RGoogleChart h -> h::acc                                                       
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
                                                                              
            let create (r : Report) = 
                    match r with
                    | Report items -> [ "<html>"
                                        renderHead items                                       
                                        renderBody items                                        
                                        "</html>" ] |> joinLines  




