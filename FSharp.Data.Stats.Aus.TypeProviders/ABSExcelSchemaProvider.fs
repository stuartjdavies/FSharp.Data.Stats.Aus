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

module ABSDataHelper = 
           let private getAll(fileName, sheetName : string) = let stream = File.Open(fileName, FileMode.Open, FileAccess.Read)     
                                                              let excelReader = ExcelReaderFactory.CreateBinaryReader(stream)
                                                              excelReader.IsFirstRowAsColumnNames <- false
              

                                                              seq {
                                                                  for line in excelReader.AsDataSet().Tables.[sheetName].Rows do
                                                                    yield (line.ItemArray |> Array.map(fun x -> x.ToString()))
                                                              } 
           let GetRows(fileName, sheetName) = getAll(fileName, sheetName) |> Seq.skip 11 |> Seq.map (fun x ->  let passed, value = Double.TryParse(x.[0])
                                                                                                               if (passed) then
                                                                                                                 x.[0] <- DateTime.FromOADate(value).ToShortDateString()
                                                                                                                 x
                                                                                                               else
                                                                                                                 x)                                            
           let GetHeaders(fileName, sheetName) = let data = getAll(fileName, sheetName) 
                                                 ((Seq.head data), (Seq.nth 2 data) ) ||> Seq.map2(fun h st -> if (h.Trim() = String.Empty) then                                                                                                                                                                   
                                                                                                                 "Date"
                                                                                                               else 
                                                                                                                 String.Format("{0} - ({1})", h, st))
                                                                                   
type ABSDataFile(fileName, sheetName) =
     let data = ABSDataHelper.GetRows(fileName, sheetName)    
     member __.Data = data

[<TypeProvider>]
type public ABSExcelSchema1Provider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to house the provided types
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let ns = "FSharp.Data.Stats.Aus.TypeProviders"

    // Create the main provided type
    let csvTy = ProvidedTypeDefinition(asm, ns, "ABSExcelSchemaProvider", Some(typeof<obj>))

    // Parameterize the type by the file to use as a template
    let filename = ProvidedStaticParameter("filename", typeof<string>)
       
    do csvTy.DefineStaticParameters([filename], fun tyName [| :? string as filename |] ->

        // resolve the filename relative to the resolution folder
        let resolvedFilename = Path.Combine(cfg.ResolutionFolder, filename)
        
        // define a provided type for each row, erasing to a float[]
        let rowTy = ProvidedTypeDefinition("Row", Some(typeof<string[]>))

        let headers = ABSDataHelper.GetHeaders(resolvedFilename,"Data1") |> Seq.toArray
          
        headers |> Seq.mapi(fun i h -> let prop = ProvidedProperty(propertyName=h, propertyType=typeof<string>, IsStatic=false,                                                                                   
                                                                                    GetterCode = (fun [row] -> <@@ (%%row:string[]).[i] @@>))
                                       prop.AddDefinitionLocation(1, i + 1, filename)
                                       prop)
                |> Seq.iter rowTy.AddMember

        // define the provided type, erasing to CsvFile
        let ty = ProvidedTypeDefinition(asm, ns, tyName, Some(typeof<ABSDataFile>))

        // add a parameterless constructor which loads the file that was used to define the schema
        ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ ABSDataFile(resolvedFilename, "Data1") @@>))

        // add a constructor taking the filename to load
        ty.AddMember(ProvidedConstructor([ProvidedParameter("filename", typeof<string>)], InvokeCode = fun [filename] -> <@@ ABSDataFile(%%filename, "Data1") @@>))
        
        // add a new, more strongly typed Data property (which uses the existing property at runtime)
        ty.AddMember(ProvidedProperty("Data", typedefof<seq<_>>.MakeGenericType(rowTy), GetterCode = fun [absDataFile] -> <@@ (%%absDataFile:ABSDataFile).Data @@>))

        // add the row type as a nested type
        ty.AddMember(rowTy)
        ty)

    // add the type to the namespace
    do this.AddNamespace(ns, [csvTy])

[<TypeProviderAssembly>]
do()

