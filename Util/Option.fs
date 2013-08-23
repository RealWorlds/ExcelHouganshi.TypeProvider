namespace Util

module Option =
  type Builder internal () =
    member this.Return(x) = Some x
    member this.Bind(x, f) = Option.bind f x

  let choice opts =
    opts |> List.tryPick (fun f -> f ())

[<AutoOpen>]
module OptionComputationExpression =
  let option = Option.Builder ()