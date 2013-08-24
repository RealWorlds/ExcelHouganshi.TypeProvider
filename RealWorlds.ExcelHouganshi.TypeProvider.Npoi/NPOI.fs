module RealWorlds.ExcelHouganshi.TypeProvider.NPOI

open RealWorlds.ExcelHouganshi.TypeProvider.Data
open NPOI
open NPOI.SS.UserModel

type NCell = ICell
type NSheet = ISheet
type NBook = IWorkbook

type NpoiCell private (cell: NCell) =
  static member Create(cell) = NpoiCell(cell) :> ExcelCell
  interface ExcelCell with
    member this.Address = ExcelAddress.ofInts (cell.RowIndex, cell.ColumnIndex)
    member this.Value
      with get () =
        match cell.CellType with
        | CellType.BLANK -> Blank
        | CellType.STRING -> Text (cell.StringCellValue)
        | CellType.NUMERIC -> Numeric (Int (int cell.NumericCellValue))
        | other -> failwithf "%A is not supported yet." other
      and set v =
        match v with
        | Blank -> cell.SetCellType(CellType.BLANK)
        | Text txt -> cell.SetCellValue(txt)
        | Numeric (Int i) -> cell.SetCellValue(float i)

type NpoiSheet private (sheet: NSheet) =
  static member Create(sheet) = NpoiSheet(sheet) :> ExcelSheet
  interface ExcelSheet with
    member this.GetCell { Row = r; Column = c } =
      match sheet.GetRow(r) with
      | null -> failwith "This sheet(%s) does not have row(%d)." sheet.SheetName r
      | row ->
          match row.GetCell(c) with
          | null -> failwith "This row(%d) of sheet(%s) does not have column(%d)." r sheet.SheetName c
          | cell -> NpoiCell.Create(cell)

open System.IO

type NpoiBook private (book: NBook, path: string) =
  static member Load(path) =
    use fs = new FileStream(path, FileMode.Open, FileAccess.Read)
    NpoiBook(NPOI.HSSF.UserModel.HSSFWorkbook(fs), path) :> ExcelBook
  interface ExcelBook with
    member this.GetSheet(sheetName) =
      match book.GetSheet(sheetName) with
      | null -> failwith "This book does not have sheet(%s). path: %s" sheetName path
      | sheet -> NpoiSheet.Create(sheet)
    member this.Save() =
      use fs = new FileStream(path, FileMode.Open, FileAccess.Write)
      book.Write(fs)