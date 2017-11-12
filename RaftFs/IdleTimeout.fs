namespace RaftFs

open System

module IdleTimeout = 

    type ActivityTimeoutInfo = {
        mutable LastReset : DateTime
        Timeout : TimeSpan
    }

    let extendTimeout info = 
        info.LastReset <- DateTime.UtcNow

    let startActivityTimeout timeout callback = 
        let info = {
            LastReset = DateTime.UtcNow;
            Timeout = timeout;
        }
        let rec waitForTimeout election = async {
            let msRemaining = int (election.LastReset + election.Timeout - DateTime.UtcNow).TotalMilliseconds
            if msRemaining <= 0 then
                callback()
            else
                do! Async.Sleep msRemaining
                do! waitForTimeout election
        }
        Async.Start(waitForTimeout info)
        info
