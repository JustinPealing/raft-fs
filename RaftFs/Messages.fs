namespace RaftFs

module Messages = 

    type AppendEntriesArguments = {
        term: int
    }

    type AppendEntriesResult = {
        success: bool
    }
