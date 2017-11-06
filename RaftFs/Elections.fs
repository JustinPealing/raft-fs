namespace RaftFs

open System

module Elections = 

    type ElectionTimeout = {
        mutable LastReset : DateTime
        Timeout : TimeSpan
    }

    let extendTimeout election = 
        election.LastReset <- DateTime.UtcNow

    let startElectionTimeout timeout callback = 
        let election = {
            LastReset = DateTime.UtcNow;
            Timeout = timeout;
        }
        let rec waitForNextElection election = async {
            let msToElectionTimeout = int (election.LastReset + election.Timeout - DateTime.UtcNow).TotalMilliseconds
            if msToElectionTimeout <= 0 then
                callback()
            else
                do! Async.Sleep msToElectionTimeout
                do! waitForNextElection election
        }
        Async.Start(waitForNextElection election)
        election
