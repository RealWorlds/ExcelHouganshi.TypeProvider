[<AutoOpen>]
module RealWorlds.ExcelHouganshi.TypeProvider.Util

type OptionBuilder () =
  member this.Return(x) = Some x
  member this.Bind(x, f) = Option.bind f x

let option = OptionBuilder ()