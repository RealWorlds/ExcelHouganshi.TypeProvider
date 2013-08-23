module Def

open RealWorlds.ExcelHouganshi.TypeProvider.Data

[<HouganshiDefinition>]
let myHouganshi =
  [
    Define.field "Title" {
      Type = typeof<string>
      Sheet = "Sheet1"
      Address = "A1"
    }
    Define.field "SubTitle" {
      Type = typeof<string>
      Sheet = "Sheet1"
      Address = "A2"
    }
  ]