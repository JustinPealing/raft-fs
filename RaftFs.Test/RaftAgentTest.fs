namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module RaftAgentTest = 
    
    [<Test>]
    let ``Initial State``() = 
        let state = RaftAgent.initialState
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(0, state.currentTerm)
        Assert.AreEqual(0, state.votedFor)

    [<Test>]
    let ``Election timeout``() = 
        let state = { state = Follower ; currentTerm = 0 ; votedFor = 0 }
        let nextState = RaftAgent.electionTimeout state
        Assert.AreEqual(Candidate, nextState.state)

    [<Test>]
    let ``ElectionTimeout via MailboxProcessor``() = Async.RunSynchronously <| async {
        do! RaftAgent.ElectionTimeout()
        let! state = RaftAgent.GetState()
        Assert.AreEqual(Candidate, state.state)
    }

    [<Test>]
    let ``Post RequestVote``() =
        let request = { term = 5; candidateId = 3; lastLogIndex = 1; lastLogTerm = 2 }
        let result = RaftAgent.RequestVote request |> Async.RunSynchronously
        Assert.AreEqual(2, result.term)
