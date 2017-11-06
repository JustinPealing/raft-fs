open NUnit.Framework
open RaftFs

/// <summary>
/// The Rpc.CreateServer function creates a server which is used to recive RPCs. It needs a
/// port (the port on which to listen).
/// </summary>
[<Test>]
let ``Create Server``() =
    let successFalse (req:Rpc.AppendEntriesArguments) : Rpc.AppendEntriesResult =
        {success = false}
    use server = Rpc.CreateServer 13000 successFalse
    Assert.NotNull(server)

/// <summary>
/// The Rpc.CreateClient function creates a client which is used to make RPCs.
/// </summary>
[<Test>]
let ``Create Client``() =
    use client = Rpc.CreateClient("localhost:13000")
    Assert.NotNull(client)

/// <summary>
/// Basic test of making a RPC using server + client.
/// </summary>
[<Test>]
let ``Make RPC``() = 
    let appendEntries (req:Rpc.AppendEntriesArguments) : Rpc.AppendEntriesResult =
        Assert.Equals(5, req.term) |> ignore
        {success = true}

    use server = Rpc.CreateServer 13000 appendEntries
    use client = Rpc.CreateClient("localhost:13000")
    let resp = client.AppendEntries {term = 5}
    Assert.IsTrue(resp.success)
