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
    | RequestVoteResult of RequestVoteArguments * RequestVoteResult
    | AppendEntiresResult of AppendEntriesArguments * AppendEntriesResult

type RaftAgent (startNewElectionTimeout, nodeId, otherNodes:OtherNode array, initialState) =

    let sendRequestVoteToAllNodes (agent:MailboxProcessor<Message>) request =
        let reply result = 
            agent.Post (RequestVoteResult (request, result))
        let send (node:OtherNode) = 
            Async.Start <| async {
                let! result = node.RequestVote request
                reply result
            }
        Seq.iter send otherNodes

    let electionTimeout (agent:MailboxProcessor<Message>) state =
        sendRequestVoteToAllNodes agent { term = 1; candidateId = 1; lastLogIndex = 1; lastLogTerm = 1}
        { state with
            state = Candidate;
            currentTerm = state.currentTerm + 1;
            votedFor = Some nodeId;
            electionTimeout = Some (startNewElectionTimeout agent) }

    let requestVote state (request:RequestVoteArguments) (rc:AsyncReplyChannel<RequestVoteResult>) =
        if (state.votedFor = None && state.currentTerm <= request.term) || state.currentTerm < request.term then
            rc.Reply { term = request.term; voteGranted = true }
            { state with
                currentTerm = request.term;
                votedFor = Some request.candidateId }
        else
            rc.Reply { term = state.currentTerm; voteGranted = false }
            state

    let processMessage agent state msg = 
        match msg with
        | GetState rc ->
            rc.Reply state
            state
        | ElectionTimeout -> electionTimeout agent state
        | RequestVote (request, rc) -> requestVote state request rc
        | AppendEntries (request, rc) ->
            rc.Reply {term = 2; success = true}
            state
        | RequestVoteResult (request, result) -> state
        | AppendEntiresResult (request, result) -> state

    let agent = MailboxProcessor<Message>.Start(fun inbox -> 
        let rec messageLoop oldState = async {
            let! msg = inbox.Receive()
            let newState = processMessage inbox oldState msg
            return! messageLoop newState
        }
        messageLoop { initialState with electionTimeout = Some (startNewElectionTimeout inbox) }
    )

    member this.GetState () = 
        agent.PostAndAsyncReply(GetState)

    member this.RequestVote request =
        agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))

    member this.AppendEntries request =
        agent.PostAndAsyncReply(fun rc -> AppendEntries (request, rc))

module RaftAgentWrapper =

    let startNewElectionTimeout minElectionTimeout (agent:MailboxProcessor<Message>) = 
        Elections.startElectionTimeout (TimeSpan.FromMilliseconds minElectionTimeout) (fun () -> agent.Post ElectionTimeout)

    let defaultState = 
        { state = Follower; currentTerm = 0; votedFor = None; electionTimeout = None }

    let createAgent minElectionTimeout nodeId state =
        match state with
        | Some actualState ->
            RaftAgent (startNewElectionTimeout minElectionTimeout, nodeId, Array.empty, actualState)
        | None ->
            RaftAgent (startNewElectionTimeout minElectionTimeout, nodeId, Array.empty, defaultState)
