namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module RpcTest = 

    /// <summary>
    /// The Rpc.CreateServer function creates a server which is used to recive RPCs. It needs a
    /// port (the port on which to listen).
    /// </summary>
    [<Test>]
    [<Ignore("Broken because of timing")>]
    let ``Create Server``() =
        let appendEntries req = {term = 1; success = false}
        let requestVote (req:RequestVoteArguments) = {term = 2; voteGranted = true}

        use server = Rpc.CreateServer 13000 appendEntries requestVote
        Assert.NotNull(server)

    /// <summary>
    /// The Rpc.CreateClient function creates a client which is used to make RPCs.
    /// </summary>
    [<Test>]
    let ``Create Client``() =
        use client = Rpc.CreateClient "localhost" 13000
        Assert.NotNull(client)

    /// <summary>
    /// Basic test of making a RPC using server + client.
    /// </summary>
    [<Test>]
    let ``Make AppendEntries RPC``() =
        let requestVote (req:RequestVoteArguments) = {term = 2; voteGranted = true}
        let appendEntries (req:AppendEntriesArguments) =
            Assert.AreEqual(5, req.term)
            Assert.AreEqual(3, req.leaderId)
            Assert.AreEqual(1, req.prevLogIndex)
            Assert.AreEqual(2, req.prevLogTerm)
            // TODO: entries is null for some reason
            Assert.AreEqual(7, req.leaderCommit)
            {term = 2; success = true}

        use server = Rpc.CreateServer 13000 appendEntries requestVote
        use client = Rpc.CreateClient "localhost" 13000
        let request = { term = 5; leaderId = 3; prevLogIndex = 1; prevLogTerm = 2; entries = Array.empty<LogEntry>; leaderCommit = 7 }
        let resp = (client.AppendEntries request) |> Async.RunSynchronously
        Assert.IsTrue(resp.success)
        Assert.AreEqual(2, resp.term)

    /// <summary>
    /// Basic test of making a RPC using server + client.
    /// </summary>
    [<Test>]
    let ``Make RequestVote RPC``() =
        let appendEntries (req:AppendEntriesArguments) = {term = 2; success = true}
        let requestVote (req:RequestVoteArguments) =
            Assert.AreEqual(5, req.term)
            Assert.AreEqual(3, req.candidateId)
            Assert.AreEqual(1, req.lastLogIndex)
            Assert.AreEqual(2, req.lastLogTerm)
            {term = 2; voteGranted = true}

        use server = Rpc.CreateServer 13000 appendEntries requestVote
        use client = Rpc.CreateClient "localhost" 13000
        let request = { term = 5; candidateId = 3; lastLogIndex = 1; lastLogTerm = 2 }
        let resp = (client.RequestVote request) |> Async.RunSynchronously
        Assert.IsTrue(resp.voteGranted)
        Assert.AreEqual(2, resp.term)
