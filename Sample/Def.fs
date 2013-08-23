module Def

open RealWorlds.ExcelHouganshi.TypeProvider.Data

[<HouganshiDefinition>]
let myHouganshi =
  [
    Define.field "Title" {
      Type = StringField
      Sheet = "Sheet1"
      Address = "A1"
    }
    Define.field "SubTitle" {
      Type = StringField
      Sheet = "Sheet1"
      Address = "A2"
    }
  ]