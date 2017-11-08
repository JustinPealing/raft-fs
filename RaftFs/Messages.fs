namespace RaftFs

open System
open ProtoBuf

module Messages = 

    [<ProtoContract; Serializable; CLIMutable>]
    type AppendEntriesArguments = {
        [<ProtoMember(1)>]
        term: int
    }

    [<ProtoContract; Serializable; CLIMutable>]
    type AppendEntriesResult = {
        [<ProtoMember(1)>]
        success: bool
    }
