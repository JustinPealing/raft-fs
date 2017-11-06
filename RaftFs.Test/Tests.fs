namespace RaftFs.Test

open NUnit.Framework

[<TestFixture>]
type Test() = 
    [<Test>]
    member this.Hello() =
        Assert.AreEqual(4, 2+2)
