#load "Setup.fsx"

open FSharp.Data

let freebase = FreebaseData.GetDataContext()

// Australian Cricket captains
freebase.Sports.Cricket.``Cricket Teams``.Individuals.``Australia national cricket team``.Captains
|> Seq.sortBy(fun c -> -c.From.AsInteger())
|> Seq.iter(fun c -> printfn "Captain - %s, DOB - %s, From - %s, To - %s" c.Captain.Name c.Captain.``Date of birth`` c.From c.To )

// English
freebase.Sports.Cricket.``Cricket Teams``.Individuals.``England cricket team``.Captains
|> Seq.sortBy(fun c -> -c.From.AsInteger())
|> Seq.iter(fun c -> printfn "Captain - %s, DOB - %s, From - %s, To - %s" c.Captain.Name c.Captain.``Date of birth`` c.From c.To )

// Indian
freebase.Sports.Cricket.``Cricket Teams``.Individuals.``India national cricket team``.Captains
|> Seq.sortBy(fun c -> -c.From.AsInteger())
|> Seq.iter(fun c -> printfn "Captain - %s, DOB - %s, From - %s, To - %s" c.Captain.Name c.Captain.``Date of birth`` c.From c.To )

