namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module RaftAgentTest = 

    /// <summary>
    /// A node is initialized to the follower state. If no messages are recieved before an election timeout
    /// then the node should become a candidate:
    /// - Changes state to "Candidate"
    /// - Increments the current term
    /// - Starts a new election timeout
    /// </summary>
    [<Test>]
    let ``Election timeout for Follower``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0)

        // Check the node state is correctly initialized
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(0, state.currentTerm)

        // Wait for the election timeout and check the state again
        do! Async.Sleep 60
        let! state2 = agent.GetState()
        Assert.AreEqual(Candidate, state2.state)
        Assert.AreEqual(1, state2.currentTerm)
    }
    
    [<Test>]
    let ``Post RequestVote``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0)
        let! result = agent.RequestVote { term = 5; candidateId = 3; lastLogIndex = 1; lastLogTerm = 2 }
        Assert.AreEqual(2, result.term)
    }
