namespace RaftFs

open System
open ProtoBuf

module Messages = 

    [<ProtoContract; Serializable; CLIMutable>]
    type LogEntry = {
        [<ProtoMember(1)>] term: int;
        [<ProtoMember(2)>] index: int;
        [<ProtoMember(3)>] command: byte array;
    }

    [<ProtoContract; Serializable; CLIMutable>]
    type AppendEntriesArguments = {
        [<ProtoMember(1)>] term: int;
        [<ProtoMember(2)>] leaderId: int;
        [<ProtoMember(3)>] prevLogIndex: int;
        [<ProtoMember(4)>] prevLogTerm: int;
        [<ProtoMember(5)>] entries: LogEntry array;
        [<ProtoMember(6)>] leaderCommit: int;
    }

    [<ProtoContract; Serializable; CLIMutable>]
    type AppendEntriesResult = {
        [<ProtoMember(1)>] term: int;
        [<ProtoMember(2)>] success: bool;
    }

    [<ProtoContract; Serializable; CLIMutable>]
    type RequestVoteArguments = {
        [<ProtoMember(1)>] term: int;
        [<ProtoMember(2)>] candidateId: int;
        [<ProtoMember(3)>] lastLogIndex: int;
        [<ProtoMember(4)>] lastLogTerm: int;
    }

    [<ProtoContract; Serializable; CLIMutable>]
    type RequestVoteResult = {
        [<ProtoMember(1)>] term: int;
        [<ProtoMember(2)>] voteGranted: bool;
    }
