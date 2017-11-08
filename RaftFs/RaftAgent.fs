namespace RaftFs

open System
open Messages

type NodeState =
    | Follower
    | Candidate
    | Leader

type State = {
    state : NodeState
    currentTerm : int
    votedFor : int
}

type Message = 
    | ElectionTimeout
    | RequestVote of RequestVoteArguments
    | AppendEntries of AppendEntriesArguments

module RaftAgent = 

    let initialState =
        { state = Follower; currentTerm = 0; votedFor = 0; }

    let electionTimeout state =
        { state with state = Candidate }

    let processMessage state msg = 
        match msg with
        | ElectionTimeout -> electionTimeout state
        | _ -> state

    let agent = MailboxProcessor.Start(fun inbox -> 
        let rec messageLoop oldState = async {
            let! msg = inbox.Receive()
            return! messageLoop (processMessage oldState msg)
        }
        messageLoop initialState
    )
