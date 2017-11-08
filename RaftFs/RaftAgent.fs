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

type RaftMessage = 
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

    type Message = RaftMessage * AsyncReplyChannel<State>

    let agent = MailboxProcessor<Message>.Start(fun inbox -> 
        let rec messageLoop oldState = async {
            let! (msg, replyChannel) = inbox.Receive()
            let newState = processMessage oldState msg
            replyChannel.Reply(newState)
            return! messageLoop newState
        }
        messageLoop initialState
    )

    let ElectionTimeout = 
        agent.PostAndAsyncReply(fun replyChannel -> ElectionTimeout, replyChannel)
