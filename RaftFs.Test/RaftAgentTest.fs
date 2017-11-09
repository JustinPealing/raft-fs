namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module RaftAgentTest = 
    
    /// <summary>
    /// If an election term times out for a node in the follower state it means that
    /// there are no known leaders, so the follower becomes a candidate and does the following:
    /// - Changes state to "Candidate"
    /// - Increments the current term
    /// - Starts a new election timeout
    /// </summary>
    [<Test>]
    let ``Election timeout for Follower``() = Async.RunSynchronously <| async {
        let agent = RaftAgent()
        agent.ElectionTimeout()
        let! state = agent.GetState()
        Assert.AreEqual(Candidate, state.state)
        Assert.AreEqual(1, state.currentTerm)
    }

    [<Test>]
    let ``ElectionTimeout via MailboxProcessor``() = Async.RunSynchronously <| async {
        let agent = RaftAgent()
        agent.ElectionTimeout()
        let! state = agent.GetState()
        Assert.AreEqual(Candidate, state.state)
    }

    [<Test>]
    let ``Post RequestVote``() = Async.RunSynchronously <| async {
        let agent = RaftAgent()
        let! result = agent.RequestVote { term = 5; candidateId = 3; lastLogIndex = 1; lastLogTerm = 2 }
        Assert.AreEqual(2, result.term)
    }
