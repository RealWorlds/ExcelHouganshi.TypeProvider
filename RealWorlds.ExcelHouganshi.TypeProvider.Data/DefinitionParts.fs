namespace RealWorlds.ExcelHouganshi.TypeProvider.Data

type FieldType = StringField | IntField

type FieldDefinition = {
  Type: FieldType
  Sheet: string
  Address: string
}

type Definition =
  | FieldDefinition of string * FieldDefinition

module Define =
  let field name info = FieldDefinition (name, info)