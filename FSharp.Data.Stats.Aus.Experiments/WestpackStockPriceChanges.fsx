#load "Setup.fsx"

open FSharp.Data
open FSharp.Charting

type Stocks = CsvProvider<"http://ichart.finance.yahoo.com/table.csv?s=WBC.AX">
let westpackStocks = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=WBC.AX")

// Look at the most recent row. Note the 'Date' property
// is of type 'DateTime' and 'Open' has a type 'decimal'
let firstRow = westpackStocks.Rows |> Seq.head
let lastDate = firstRow.Date
let lastOpen = firstRow.Open

// Visualize the stock prices
[ for row in westpackStocks.Rows -> row.Date, row.Open ]
|> Chart.FastLine