namespace RealWorlds.ExcelHouganshi.TypeProvider.Data

open System

type FieldDefinition = {
  Type: Type
  Sheet: string
  Address: string
}

type Definition =
  | FieldDefinition of string * FieldDefinition

module Define =
  let field name info = FieldDefinition (name, info)