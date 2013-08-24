namespace RealWorlds.ExcelHouganshi.TypeProvider.Data

open System

type ExcelAddress = {
  Row: int
  Column: int
}
with
  override this.ToString() = sprintf "%A" this

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ExcelAddress =
  let ofA1Format a1FormatAddress =
    let r = String.Concat(a1FormatAddress |> Seq.skipWhile (fun c -> 'A' <= c && c <= 'Z'))
    let c = a1FormatAddress |> Seq.takeWhile (fun c -> 'A' <= c && c <= 'Z')
    { Row = int r - 1; Column = (c |> Seq.fold (fun acc x -> acc * 26 + (int x - (int 'A') + 1)) 0) - 1 }

  let ofInts (row, col) = { Row = row; Column = col }
  let toInts address = (address.Row, address.Column)

type ExcelNumericValue =
  | Int of int

type ExcelValue =
  | Blank
  | Text of string
  | Numeric of ExcelNumericValue

type ExcelCell =
  abstract member Address: ExcelAddress with get
  abstract member Value: ExcelValue with get, set

type ExcelSheet =
  abstract member GetCell: ExcelAddress -> ExcelCell

type ExcelBook =
  abstract member GetSheet: string -> ExcelSheet
  abstract member Save: unit -> unit

[<AutoOpen>]
module Ext =
  type ExcelCell with
    member this.Value with set (v: string) = this.Value <- Text v
    member this.Value with set (v: int) = this.Value <- Numeric (Int v)