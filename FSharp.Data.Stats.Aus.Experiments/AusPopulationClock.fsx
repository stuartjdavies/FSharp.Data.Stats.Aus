#load "Setup.fsx"

open FSharp.Data

type ausPopClock = JsonProvider<"""{"attribution":"Australian Bureau of Statistics","popNow":"23628647","timeStamp":"28 Sep 2014 21:08:51 EST","projectionStartDate":"31 March 2014","birthRate":"1 minute  and 43 seconds ","deathRate":"3 minutes  and 31 seconds ","overseasMigrationRate":"2 minutes  and 5 seconds ","growthRate":"1 minute  and 17 seconds ","rateSecond":"77.03920850128252106998","source":"Australian Demographic Statistics, March Quarter 2014 (cat. no. 3101.0)","sourceURL":"http://www.abs.gov.au/ausstats/abs@.nsf/mf/3101.0","copyRight":"Copyright Commonwealth of Australia"}""">
let json = Http.RequestString("http://www.abs.gov.au/api/demography/populationprojection")  

// let doc = ausPopClock.Load("""http://www.abs.gov.au/api/demography/populationprojection""")
let doc = ausPopClock.Parse(json)

printf "Birth rate %s" doc.BirthRate
printf "Rate per second %f" doc.RateSecond
printf "Growth Rate %s" doc.GrowthRate
printf "Source %s" doc.Source
printf "Rate per second %f" doc.RateSecond
printf "Projection Start Date %s" (doc.ProjectionStartDate.ToString())