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
    votedFor : int option
    electionTimeout : Elections.ElectionTimeout option
}

type Message = 
    | GetState of AsyncReplyChannel<State>
    | ElectionTimeout
    | RequestVote of RequestVoteArguments * AsyncReplyChannel<RequestVoteResult>
    | AppendEntries of AppendEntriesArguments * AsyncReplyChannel<AppendEntriesResult>

type RaftAgent(minElectionTimeout, maxElectionTimeout, nodeId, initialState) =

    let electionTimeout state =
        { state with
            state = Candidate;
            currentTerm = state.currentTerm + 1;
            votedFor = Some nodeId }

    let requestVote state (request:RequestVoteArguments) (rc:AsyncReplyChannel<RequestVoteResult>) =
        if (state.votedFor = None && state.currentTerm <= request.term) || state.currentTerm < request.term then
            rc.Reply { term = request.term; voteGranted = true }
            { state with
                currentTerm = request.term;
                votedFor = Some request.candidateId }
        else
            rc.Reply { term = state.currentTerm; voteGranted = false }
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
        let electionTimeout = 
            Elections.startElectionTimeout (TimeSpan.FromMilliseconds minElectionTimeout) (fun () -> inbox.Post ElectionTimeout)
        messageLoop { initialState with electionTimeout = Some electionTimeout }
    )

    new(minElectionTimeout, maxElectionTimeout, nodeId) =
        RaftAgent(minElectionTimeout, maxElectionTimeout, nodeId, { state = Follower; currentTerm = 0; votedFor = None; electionTimeout = None })

    member this.GetState() = 
        agent.PostAndAsyncReply(GetState)

    member this.RequestVote request =
        agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))

    member this.AppendEntries request =
        agent.PostAndAsyncReply(fun rc -> AppendEntries (request, rc))
