namespace RealWorlds.ExcelHouganshi.TypeProvider.FSharpCodeCompiler

open System
open System.IO
open System.Reflection
open System.CodeDom.Compiler
open Microsoft.FSharp.Compiler.CodeDom

type CompileResult<'T> =
  | Failure of CompilerError seq
  | Success of 'T

[<AutoOpen>]
module FSharpCodeCompiler =
  let compile outputFilePath refAssemblies src =
    use provider = new FSharpCodeProvider()
    let param = CompilerParameters()
    param.OutputAssembly <- outputFilePath
    param.ReferencedAssemblies.AddRange(Array.ofList refAssemblies)
    let res = provider.CompileAssemblyFromSource(param, [| src |])
    let errors = res.Errors |> Seq.cast<CompilerError>
    if not (Seq.isEmpty errors) then
      Failure errors
    else
      let asm = res.CompiledAssembly
      Success (asm.GetTypes())