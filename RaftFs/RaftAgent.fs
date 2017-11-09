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
    electionTimeout : Elections.ElectionTimeout
}

type Message = 
    | GetState of AsyncReplyChannel<State>
    | ElectionTimeout
    | RequestVote of RequestVoteArguments * AsyncReplyChannel<RequestVoteResult>
    | AppendEntries of AppendEntriesArguments * AsyncReplyChannel<AppendEntriesResult>

type RaftAgent(minElectionTimeout, maxElectionTimeout) =

    let electionTimeout state =
        { state with
            state = Candidate;
            currentTerm = state.currentTerm + 1 }

    let requestVote state (request:RequestVoteArguments) (rc:AsyncReplyChannel<RequestVoteResult>) =
        if state.votedFor = 0 then
            rc.Reply { term = request.term; voteGranted = true }
            { state with
                currentTerm = request.term;
                votedFor = request.candidateId }
        else
            rc.Reply { term = request.term; voteGranted = false }
            state

    let processMessage state msg = 
        match msg with
        | GetState rc ->
            rc.Reply state
            state
        | ElectionTimeout -> electionTimeout state
        | RequestVote (request, rc) -> requestVote state request rc
        | AppendEntries (request, rc) ->
            rc.Reply {term = 2; success = true}
            state

    let agent = MailboxProcessor<Message>.Start(fun inbox -> 
        let rec messageLoop oldState = async {
            let! msg = inbox.Receive()
            let newState = processMessage oldState msg
            return! messageLoop newState
        }
        let initialState = { state = Follower; currentTerm = 0; votedFor = 0;
          electionTimeout = Elections.startElectionTimeout (TimeSpan.FromMilliseconds minElectionTimeout) (fun () -> inbox.Post ElectionTimeout) }

        messageLoop initialState
    )

    member this.GetState() = 
        agent.PostAndAsyncReply(GetState)

    member this.RequestVote request =
        agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))

    member this.AppendEntries request =
        agent.PostAndAsyncReply(fun rc -> AppendEntries (request, rc))
