[<NUnit.Framework.TestFixture>]
module ExcelTest

open RealWorlds.ExcelHouganshi.TypeProvider.Data
open NUnit.Framework
open FsUnit

[<TestCase("A1", 0, 0)>]
[<TestCase("Z1", 0, 25)>]
[<TestCase("AA1", 0, 26)>]
[<TestCase("AB1", 0, 27)>]
[<TestCase("A10", 9, 0)>]
[<TestCase("Z100", 99, 25)>]
[<TestCase("AA1000", 999, 26)>]
[<TestCase("AB1000", 999, 27)>]
let ``ExcelAddress.ofA1Format`` address expectedRow expectedCol =
  (ExcelAddress.ofA1Format address) |> should equal { Row = expectedRow; Column = expectedCol }