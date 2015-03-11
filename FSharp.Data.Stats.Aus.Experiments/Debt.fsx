#load "Setup.fsx"
 
open System
open RDotNet
open RProvider
open FSharp.Data
open RProvider.``base``
open RProvider.graphics
open System.Collections.Generic

let wb = WorldBankData.GetDataContext()

let ausEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Australia.Indicators.``Central government debt, total (% of GDP)``.[y]]

let italyEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Italy.Indicators.``Central government debt, total (% of GDP)``.[y]] 
let germanyEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Germany.Indicators.``Central government debt, total (% of GDP)``.[y]] 
let usEmploy = [ for y in 2000 .. 2014 -> wb.Countries.``United States``.Indicators.``Central government debt, total (% of GDP)``.[y]] 
let ukEmploy = [ for y in 2000 .. 2014 -> wb.Countries.``United Kingdom``.Indicators.``Central government debt, total (% of GDP)``.[y]] 
let spainEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Spain.Indicators.``Central government debt, total (% of GDP)``.[y]] 
let norwayEmploy = [ for y in 2000 .. 2014 -> wb.Countries.Norway.Indicators.``Central government debt, total (% of GDP)``.[y]] 


let g_range = R.range(0, ausEmploy, italyEmploy)   

R.plot(namedParams [   
        "x", box ausEmploy; 
        "type", box "o"; 
        "col", box "blue";        
        "ylim", box [| 15; 140|];
        "ann", box false;])

R.box()

R.lines(namedParams [   
            "x", box italyEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4;
            "col", box "red";])

R.lines(namedParams [   
            "x", box germanyEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4; 
            "col", box "green";])

R.lines(namedParams [   
            "x", box usEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4;             
            "col", box "purple";])

R.lines(namedParams [   
            "x", box spainEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4;             
            "col", box "grey";])

R.lines(namedParams [   
            "x", box ukEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4;             
            "col", box "brown";])

R.lines(namedParams [   
            "x", box norwayEmploy; 
            "type", box "b";
            "pch", box 22;  
            "lty", box 2;
            "lwd", box 1.4;             
            "col", box "orange";])

R.title(main="Central government debt, total (% of GDP)")               
R.title(xlab="year")
R.title(ylab="% of GDP")

R.legend("topleft",
         legend=[|"Aus";"Italy";"Germany";"Us";"Spain";"Uk";"Norway"|],
         col=[|"blue";"red";"green";"purple";"grey";"brown";"orange"|],
         lty=[|1;1|],
         ncol=3)




