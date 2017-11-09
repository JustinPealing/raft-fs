namespace RaftFs.Test

open System
open NUnit.Framework
open RaftFs

module ElectionTimeoutTest = 
    
    /// <summary>
    /// With no other interaction an election timeout should trigger after the given time interval.
    /// </summary>
    [<Test>]
    let ``Election timeout``() = Async.RunSynchronously <| async {
        let start = DateTime.UtcNow
        let mutable endTime = DateTime.MinValue
        let callback() =
            endTime <- DateTime.UtcNow
        Elections.startElectionTimeout (TimeSpan.FromMilliseconds 100.0) callback |> ignore
        Assert.AreEqual(DateTime.MinValue, endTime)
        do! Async.Sleep 110
        Assert.IsTrue((endTime - start).TotalMilliseconds > 0.0)
    }
