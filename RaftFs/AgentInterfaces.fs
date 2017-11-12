namespace RaftFs

open RaftFs.Messages
open RaftFs.IdleTimeout

type NodeLeaderState =
    | Follower
    | Candidate
    | Leader

type State = {
    state : NodeLeaderState
    currentTerm : int
    votedFor : int option
    electionTimeout : ActivityTimeoutInfo option
}

type IRaftNode =
    
    abstract member GetState : unit -> Async<State>
    abstract member RequestVote : RequestVoteArguments -> Async<RequestVoteResult>
    abstract member AppendEntries : AppendEntriesArguments -> Async<AppendEntriesResult>
    abstract member RequestVoteResult : RequestVoteArguments -> RequestVoteResult -> unit

type IRemoteRaftNode = 

    abstract member RequestVote : RequestVoteArguments -> unit
