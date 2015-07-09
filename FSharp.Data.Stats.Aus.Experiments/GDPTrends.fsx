#load "Setup.fsx"

open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic

let wb = WorldBankData.GetDataContext()

let ausGDP = [ for y in 2000 .. 2015 -> wb.Countries.Australia.Indicators.``GDP per capita, PPP (current international $)``.[y]]
let italyGDP = [ for y in 2000 .. 2015 -> wb.Countries.Italy.Indicators.``GDP per capita, PPP (current international $)``.[y]] 
let germanyGDP = [ for y in 2000 .. 2015 -> wb.Countries.Germany.Indicators.``GDP per capita, PPP (current international $)``.[y]] 
let usGDP = [ for y in 2000 .. 2015 -> wb.Countries.``United States``.Indicators.``GDP per capita, PPP (current international $)``.[y]] 
let ukGDP = [ for y in 2000 .. 2015 -> wb.Countries.``United Kingdom``.Indicators.``GDP per capita, PPP (current international $)``.[y]] 
let spainGDP = [ for y in 2000 .. 2015 -> wb.Countries.Spain.Indicators.``GDP per capita, PPP (current international $)``.[y]] 
let norwayGDP = [ for y in 2000 .. 2015 -> wb.Countries.Norway.Indicators.``GDP per capita, PPP (current international $)``.[y]] 

let g_range = R.range(0, ausGDP, italyGDP)   

R.plot(namedParams [   
        "x", box ausGDP; 
        "type", box "o"; 
        "col", box "blue";        
        //"ylim", box [| 0; 30|];
        "ann", box false;])

R.box()

R.lines(namedParams [   
            "x", box italyGDP; 
            "type", box "o";
            "pch", box 22;  
            "lty", box 2 
            "col", box "red";])

R.lines(namedParams [   
            "x", box germanyGDP; 
            "type", box "o"; 
            "col", box "green";])

R.lines(namedParams [   
            "x", box usGDP; 
            "type", box "o"; 
            "col", box "purple";])

R.lines(namedParams [   
            "x", box spainGDP; 
            "type", box "o"; 
            "col", box "grey";])

R.lines(namedParams [   
            "x", box ukGDP; 
            "type", box "o"; 
            "col", box "brown";])

R.lines(namedParams [   
            "x", box norwayGDP; 
            "type", box "o"; 
            "col", box "orange";])

R.title(main="GDP per capita, PPP (current international $)")               
R.title(xlab="year")
R.title(ylab="current international $")

R.legend("topleft",
         legend=[|"Aus";"Italy";"Germany";"Us";"Spain";"Uk";"Norway"|],
         col=[|"blue";"red";"green";"purple";"grey";"brown";"orange"|],
         //lty=[|1;1|],
         ncol=3)