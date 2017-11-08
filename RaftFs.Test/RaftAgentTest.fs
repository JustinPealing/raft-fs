namespace RaftFs.Test

open NUnit.Framework
open RaftFs

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
