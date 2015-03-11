#load "Setup.fsx"

open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic

let wb = WorldBankData.GetDataContext()

let ausEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Australia.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]]
let italyEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Italy.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 
let germanyEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Germany.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 
let usEmploy = [ for y in 2000 .. 2014 -> wb.Countries.``United States``.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 
let ukEmploy = [ for y in 2000 .. 2014 -> wb.Countries.``United Kingdom``.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 
let spainEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Spain.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 
let norwayEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Norway.Indicators.``Unemployment, total (% of total labor force) (national estimate)``.[y]] 

let g_range = R.range(0, ausEmploy, italyEmploy)   

R.plot(namedParams [   
        "x", box ausEmploy; 
        "type", box "o"; 
        "col", box "blue";        
        "ylim", box [| 0; 30|];
        "ann", box false;])

R.box()

R.lines(namedParams [   
            "x", box italyEmploy; 
            "type", box "o";
            "pch", box 22;  
            "lty", box 2 
            "col", box "red";])

R.lines(namedParams [   
            "x", box germanyEmploy; 
            "type", box "o"; 
            "col", box "green";])

R.lines(namedParams [   
            "x", box usEmploy; 
            "type", box "o"; 
            "col", box "purple";])

R.lines(namedParams [   
            "x", box spainEmploy; 
            "type", box "o"; 
            "col", box "grey";])

R.lines(namedParams [   
            "x", box ukEmploy; 
            "type", box "o"; 
            "col", box "brown";])

R.lines(namedParams [   
            "x", box norwayEmploy; 
            "type", box "o"; 
            "col", box "orange";])

R.title(main="Unemployment, total (% of total labor force) (national estimate)")               
R.title(xlab="year")
R.title(ylab="% of total labor force")

R.legend("topleft",
         legend=[|"Aus";"Italy";"Germany";"Us";"Spain";"Uk";"Norway"|],
         col=[|"blue";"red";"green";"purple";"grey";"brown";"orange"|],
         lty=[|1;1|],
         ncol=3)



