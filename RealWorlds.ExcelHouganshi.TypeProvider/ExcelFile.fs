namespace RealWorlds.ExcelHouganshi.TypeProvider

open System

open RealWorlds.ExcelHouganshi.TypeProvider.Data

type ExcelFile (book: ExcelBook) =
  member this.RawOp(f) = f book
  member this.RawRead(cell: ExcelCell, typ: Type): obj =
    let value = cell.Value
    if typ = typeof<string> then
      match value with
      | Blank -> ""
      | Text text -> text
      | Numeric (Int i) -> string i
      |> box
    else
      failwithf "%s isn't supported yet." typ.Name
  member this.RawWrite(cell: ExcelCell, obj: obj, typ: Type) =
    cell.Value <-
      if typ = typeof<string> then
        Text (obj :?> string)
      else
        failwithf "%s isn't supported yet." typ.Name
