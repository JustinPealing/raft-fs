namespace RaftFs

open RaftFs.Messages

type NodeState =
    | Follower
    | Candidate
    | Leader

type State = {
    state : NodeState
    currentTerm : int
    votedFor : int option
    electionTimeout : Elections.ElectionTimeout option
}

type IRaftAgent =
    
    abstract member GetState : unit -> Async<State>
    abstract member RequestVote : RequestVoteArguments -> Async<RequestVoteResult>
    abstract member AppendEntries : AppendEntriesArguments -> Async<AppendEntriesResult>
