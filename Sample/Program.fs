// F# の詳細については、http://fsharp.net を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。

open RealWorlds.ExcelHouganshi.TypeProvider

type MyHouganshi = Houganshi<"Def.fs">

[<EntryPoint>]
let main argv =
  let x = MyHouganshi("sample.xls", NPOI.NpoiBook.Load)
  printfn "%A" x.Title
  printfn "%A" x.SubTitle
  x.SubTitle <- "hoge"
  printfn "%A" x.Title
  printfn "%A" x.SubTitle
  x.Save()
  0 // 整数の終了コードを返します
