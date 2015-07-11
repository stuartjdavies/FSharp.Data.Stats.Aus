namespace FSharp.Data.Stats.Aus.TypeProviders

#nowarn "0025"

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open ProviderImplementation.ProvidedTypes
open System.Text.RegularExpressions
open System.IO
open System.Net

module RBADataHelper = 
           let downloadPageAsString(url : string) = (new WebClient()).DownloadString url 

           let findHeaderIndex (lines : string array) = 
                    lines |> Array.findIndex(fun line -> line.Trim().StartsWith("Series ID") = true)

           let getHeaders (line : string) =
                    let colTitles =line.Split(',') |> Array.filter(fun column -> column.Trim() <> String.Empty)
                                                 |> Array.map(fun column -> column.Trim())
                    Array.append [| "Series ID" |] colTitles                

           let getRows(columnEndIndex, rows : string array) =
                    rows |> Array.filter(fun row -> row.Length > 2 && row.[0..2].Trim() <> ",,")
                         |> Array.map(fun row -> row.Split(',') |> Array.map(fun col -> col.Trim()))
                         |> Array.map(fun row -> row.[0 .. columnEndIndex ])                      
                                                                                                                                  
type RBADataFile(data : (string[]) seq) =                   
           member __.Data = data

[<TypeProvider>]
type public RBACsvStatisticsProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to house the provided types
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let ns = "FSharp.Data.Stats.Aus.TypeProviders"

    // Create the main provided type
    let csvTy = ProvidedTypeDefinition(asm, ns, "RBACsvStatisticsProvider", Some(typeof<obj>))

    // Parameterize the type by the file to use as a template
    let filename = ProvidedStaticParameter("filename", typeof<string>)
       
    do csvTy.DefineStaticParameters([filename], fun tyName [| :? string as filename |] ->        
        let parseDateTime row i =
            <@@ 
                DateTime.Parse((%%row:string[]).[i])                     
            @@>

        let parseSingle row i =
            <@@ 
                try Single.Parse((%%row:string[]).[i])
                with 
                | _ -> Single.NaN                 
            @@>

        // resolve the filename relative to the resolution folder
        let resolvedFilename = Path.Combine(cfg.ResolutionFolder, filename)
        
        // define a provided type for each row, erasing to a float[]
        let rowTy = ProvidedTypeDefinition("Row", Some(typeof<string[]>))

        let lines = RBADataHelper.downloadPageAsString(filename).Split('\r')
        let headerIndex = RBADataHelper.findHeaderIndex(lines)
        let headers = RBADataHelper.getHeaders(lines.[1])    
                       
        rowTy.AddMember(ProvidedProperty(headers.[0], typeof<DateTime>, GetterCode = fun [row] -> parseDateTime row 0))
       
        headers.[1..] |> Seq.mapi(fun i h -> ProvidedProperty(h, typeof<Single>, GetterCode = fun [row] -> parseSingle row (i + 1)))
                      |> Seq.iter rowTy.AddMember
            
        let rows = RBADataHelper.getRows(headers.Length - 1, lines.[(headerIndex + 1) ..])
                  
        // define the provided type, erasing to CsvFile
        let ty = ProvidedTypeDefinition(asm, ns, tyName, Some(typeof<RBADataFile>))

        // add a parameterless constructor which loads the file that was used to define the schema
        ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ RBADataFile(rows) @@>))

        // add a constructor taking the filename to load
        // ty.AddMember(ProvidedConstructor([ProvidedParameter("filename", typeof<string>)], InvokeCode = fun [filename] -> <@@ ABSDataFile(%%filename, "Data1") @@>))
        
        // add a new, more strongly typed Data property (which uses the existing property at runtime)
        ty.AddMember(ProvidedProperty("Data", typedefof<seq<_>>.MakeGenericType(rowTy), GetterCode = fun [rbsDataFile] -> <@@ (%%rbsDataFile:RBADataFile).Data @@>))

        // add the row type as a nested type
        ty.AddMember(rowTy)
        ty)

    // add the type to the namespace
    do this.AddNamespace(ns, [csvTy])

[<TypeProviderAssembly>]
do()

