﻿namespace RealWorlds.ExcelHouganshi.TypeProvider

open System
open System.IO
open System.Reflection

open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open Util

open Microsoft.FSharp.Quotations

open RealWorlds.ExcelHouganshi.TypeProvider.Data
open RealWorlds.ExcelHouganshi.TypeProvider.FSharpCodeCompiler

[<AutoOpen>]
module Impl =
  let ns = "RealWorlds.ExcelHouganshi.TypeProvider"
  let createTempDir tmpDirRoot =
    let tmpDir = Path.Combine(tmpDirRoot, ns)
    if not <| Directory.Exists(tmpDir) then
      Directory.CreateDirectory(tmpDir) |> ignore
    tmpDir

  let compileIfNeed tmpDir dataDll definitionFilePath =
    let src = File.ReadAllText(definitionFilePath)
    let sha1 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(src, "sha1")
    let outputFilePath = Path.Combine(tmpDir, sha1 + ".dll")
    if File.Exists(outputFilePath) then
      Assembly.LoadFrom(outputFilePath).GetTypes()
    else
      let refAsms = [dataDll]
      match FSharpCodeCompiler.compile outputFilePath refAsms src with
      | Success types -> types
      | Failure errors -> failwithf "%A" errors

  let compileDefinition tmpDir dataDll definitionFilePath =
    let types = compileIfNeed tmpDir dataDll definitionFilePath
    let definition =
      let isHouganshiDefinition (m: MemberInfo) =
        m.GetCustomAttributes<HouganshiDefinition>()
        |> Seq.isEmpty
        |> not
      let definitionMember (t: Type) =
        t.GetMembers() |> Seq.tryFind isHouganshiDefinition
      let tryGetStaticValueFrom (m: MemberInfo) =
        match m with
        | :? FieldInfo as field -> field.GetValue(null) |> Some
        | :? PropertyInfo as prop -> prop.GetValue(null) |> Some
        | :? MethodInfo as m -> m.Invoke(null, [||]) |> Some
        | _ -> None
      option {
        let! defMember = types |> Seq.tryPick definitionMember
        let! value = tryGetStaticValueFrom defMember
        return value
      }
    match definition with
    | Some v -> v :?> Definition list
    | None -> failwithf "%s has no %s" definitionFilePath (typeof<HouganshiDefinition>.Name)

  let addCtor memberDefs (typ: ProvidedTypeDefinition) =
    typ.AddMember(
      ProvidedConstructor(
        [ProvidedParameter("path", typeof<string>); ProvidedParameter("loader", typeof<string -> ExcelBook>)],
        InvokeCode = function [pathExpr; loaderExpr] -> <@@ ExcelFile((%%loaderExpr : string -> ExcelBook) %%pathExpr) @@> | _ -> failwith "oops!"
      )
    )
    typ

  let addMember (typ: ProvidedTypeDefinition) = function
  | FieldDefinition (name, ({ Type = fieldType; Sheet = sheet; Address = address } as fieldDef)) ->
      let ft, typeName =
        match fieldType with
        | StringField -> typeof<string>, "string"
        | IntField -> typeof<int>, "int"
      let prop = ProvidedProperty(name, ft)
      prop.GetterCode <- fun args ->
        <@@
          let this = %%args.[0] : ExcelFile
          this.RawOp(fun book ->
            this.RawRead(book.GetSheet(sheet).GetCell(ExcelAddress.ofA1Format address), typeName)
          )
        @@>
      let rawOp = typeof<ExcelFile>.GetMethod("RawOp").MakeGenericMethod([| typeof<unit> |])
      let rawWrite = typeof<ExcelFile>.GetMethod("RawWrite")
      let getSheet = typeof<ExcelBook>.GetMethod("GetSheet")
      let getCell = typeof<ExcelSheet>.GetMethod("GetCell")
      prop.SetterCode <- fun args ->
        let thisV = Var("this", typeof<ExcelFile>)
        let valueV = Var("value", ft)
        let bookV = Var("book", typeof<ExcelBook>)
        let thisExpr = Expr.Var(thisV)
        let valueExpr = Expr.Var(valueV)
        let bookExpr = Expr.Var(bookV)
        // 型指定のスプライスができないので、式木を手で構築する
        (*
          let this = %%args.[0] : ExcelFile
          let value = %%args.[1] : %%ft <ここが書けない>
          this.RawOp(fun book ->
            this.RawWrite(book.GetSheet(sheet).GetCell(ExcelAddress.ofA1Format address), value, typeName)
          )
        *)
        Expr.Let(thisV, args.[0], 
          Expr.Let(valueV, args.[1],
            Expr.Call(thisExpr, rawOp,
              [
                Expr.Lambda(
                  bookV,
                  Expr.Call(thisExpr, rawWrite,
                    [
                      <@@ (%%bookExpr: ExcelBook).GetSheet(sheet).GetCell(ExcelAddress.ofA1Format address) @@>;
                      valueExpr;
                      <@@ typeName @@> ]))
              ])))
      typ.AddMember(prop)

  let addMembers memberDefs (typ: ProvidedTypeDefinition) =
    memberDefs |> Seq.iter (addMember typ)
    typ

[<TypeProvider>]
type Houganshi (config: TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces ()

  let asm = Assembly.GetExecutingAssembly()

  let typ = ProvidedTypeDefinition(asm, ns, "Houganshi", Some typeof<obj>, HideObjectMethods = true)
  let tmpDir = createTempDir config.TemporaryFolder
  let dataDll = config.ReferencedAssemblies |> Array.find (fun asm -> Path.GetFileName(asm) = ns + ".Data.dll")

  do typ.DefineStaticParameters(
      [ProvidedStaticParameter("definitionFilePath", typeof<string>)],
      fun typeName parameters ->
        match parameters with
        | [| :? string as definitionFilePath |] ->
            let definitionFilePath = Path.GetFullPath(Path.Combine(config.ResolutionFolder, definitionFilePath))
            let memberDefs = compileDefinition tmpDir dataDll definitionFilePath

            let typ =
              ProvidedTypeDefinition(asm, ns, typeName, Some typeof<ExcelFile>, HideObjectMethods = true)
              |> addCtor memberDefs
              |> addMembers memberDefs
            typ
        | _ -> failwith "Invalid parameter"
  )

  do this.AddNamespace(ns, [typ])