namespace RealWorlds.ExcelHouganshi.TypeProvider

open RealWorlds.ExcelHouganshi.TypeProvider.Data

type ExcelFile (book: ExcelBook) =
  member this.Save() = book.Save()
  member this.RawOp(f) = f book
  member this.RawRead(cell: ExcelCell, typeStr): obj =
    let value = cell.Value
    match typeStr with
    | "string" ->
        match value with
        | Blank -> ""
        | Text text -> text
        | Numeric (Int i) -> string i
        |> box
    | "int" ->
        match value with
        | Blank -> 0
        | Text text -> int text
        | Numeric (Int i) -> i
        |> box
    | other -> failwithf "%s isn't supported yet." other

  member this.RawWrite(cell: ExcelCell, obj: obj, typeStr) =
    cell.Value <-
      match typeStr with
      | "string" -> Text (obj :?> string)
      | "int" -> Numeric (Int (obj :?> int))
      | other -> failwithf "%s isn't supported yet." other
