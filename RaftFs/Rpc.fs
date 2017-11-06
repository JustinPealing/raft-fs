namespace RaftFs

open System

module Rpc = 

    type AppendEntriesArguments = {
        term: int
    }

    type AppendEntriesResult = {
        success: bool
    }

    type IRpcClient =
        inherit IDisposable
        abstract member AppendEntries: AppendEntriesArguments -> AppendEntriesResult

    let CreateServer port appendEntries =
        {new IDisposable with
            member this.Dispose() = ()}

    let CreateClient address = 

        let appendEntires (req:AppendEntriesArguments) : AppendEntriesResult =
            {success = false}

        {new IRpcClient with
            member this.Dispose() = ()
            member this.AppendEntries req = appendEntires req }
