namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module RaftAgentTest = 

    /// <summary>
    /// A node is initialized to the follower state. If no messages are recieved before an election timeout
    /// then the node should become a Candidate.
    /// </summary>
    [<Test>]
    let ``Election timeout for Follower``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0, 7)

        // Check the node state is correctly initialized
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(0, state.currentTerm)

        // Wait for the election timeout and check the state again
        do! Async.Sleep 60
        let! state = agent.GetState()
        Assert.AreEqual(Candidate, state.state)
        Assert.AreEqual(1, state.currentTerm)
        Assert.AreEqual(Some 7, state.votedFor)
    }
    
    /// <summary>
    /// Followers recieving RequestVote RPCs should give their vote if the Candidates term is greater than or equal
    /// to the current term and they have not yet voted for another Candidate in this election term.
    /// </summary>
    [<Test>]
    let ``RequestVote from Candidate``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0, 7)

        // Send the RequestVote RPC and check the response
        let! result = agent.RequestVote { term = 1; candidateId = 2; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.AreEqual(1, result.term)
        Assert.IsTrue(result.voteGranted)
        
        // Check that the node state has also been updated
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(1, state.currentTerm)
        Assert.AreEqual(Some 2, state.votedFor)
    }
    
    /// <summary>
    /// Followers should not grant their vote to two different candidates in the same term.
    /// </summary>
    [<Test>]
    let ``Vote not granted to two candidates in the same term``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0, 7)

        // Candidate 2 sends a RequestVote RPC, which should result in a granted vote
        let! result = agent.RequestVote { term = 1; candidateId = 2; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.IsTrue(result.voteGranted, "Vote not granted to the first candidate")

        // This means that Candidate 3 should not have their vote granted
        let! result = agent.RequestVote { term = 1; candidateId = 3; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.IsFalse(result.voteGranted, "Vote granted to a second candidate")
        
        // Check that the node state is correct
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(1, state.currentTerm)
        Assert.AreEqual(Some 2, state.votedFor)
    }
    
    /// <summary>
    /// A vote should not be granted to a Candidate with an earlier term than the current one.
    /// </summary>
    [<Test>]
    let ``Votes not granted to earlier term``() = Async.RunSynchronously <| async {
        let state = { state = Follower; currentTerm = 2; votedFor = None; electionTimeout = None }
        let agent = RaftAgent(50.0, 50.0, 7, state)

        let! result = agent.RequestVote { term = 1; candidateId = 3; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.IsFalse(result.voteGranted)
        Assert.AreEqual(2, result.term)
        
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(2, state.currentTerm)
        Assert.AreEqual(None, state.votedFor)
    }
    
    /// <summary>
    /// Votes should be granted to Candidates in later terms, even if a vote has already been granted to another
    /// candidate in the current term.
    /// </summary>
    [<Test>]
    let ``Votes granted to later terms``() = Async.RunSynchronously <| async {
        let agent = RaftAgent(50.0, 50.0, 7)

        // Candidate 2 sends a RequestVote RPC, which should result in a granted vote
        let! result = agent.RequestVote { term = 1; candidateId = 2; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.IsTrue(result.voteGranted, "Vote not granted to the first candidate")

        // However Candidate 3 has a later term, so the vote should be overidden
        let! result = agent.RequestVote { term = 2; candidateId = 3; lastLogIndex = 0; lastLogTerm = 0 }
        Assert.IsTrue(result.voteGranted, "Vote not granted to a later term")
        
        let! state = agent.GetState()
        Assert.AreEqual(Follower, state.state)
        Assert.AreEqual(2, state.currentTerm)
        Assert.AreEqual(Some 3, state.votedFor)
    }

    // TODO:
    // - RequestVote RPC test cases for Candidates and Leaders
    // - Vote granted to self when convert to Follower
    // - Test cases when election timeout is restarted
