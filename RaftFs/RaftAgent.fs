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
    | GetState of AsyncReplyChannel<State>
    | ElectionTimeout of AsyncReplyChannel<unit>
    | RequestVote of RequestVoteArguments * AsyncReplyChannel<RequestVoteResult>
    | AppendEntries of AppendEntriesArguments * AsyncReplyChannel<AppendEntriesResult>

module RaftAgent = 

    let initialState =
        { state = Follower; currentTerm = 0; votedFor = 0; }

    let electionTimeout state =
        { state with state = Candidate }

    let processMessage state msg = 
        match msg with
        | GetState rc ->
            rc.Reply state
            state
        | ElectionTimeout rc ->
            let newState = electionTimeout state
            rc.Reply()
            newState
        | RequestVote (request, rc) ->
            rc.Reply {term = 2; voteGranted = true}
            state
        | AppendEntries (request, rc) ->
            rc.Reply {term = 2; success = true}
            state

    let agent = MailboxProcessor<Message>.Start(fun inbox -> 
        let rec messageLoop oldState = async {
            let! msg = inbox.Receive()
            let newState = processMessage oldState msg
            return! messageLoop newState
        }
        messageLoop initialState
    )

    let GetState() = 
        agent.PostAndAsyncReply(GetState)

    let ElectionTimeout() = 
        agent.PostAndAsyncReply(ElectionTimeout)

    let RequestVote request =
        agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))

    let AppendEntries request =
        agent.PostAndAsyncReply(fun rc -> AppendEntries (request, rc))
