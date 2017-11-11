namespace RaftFs

open System
open Messages

type Message = 
    | GetState of AsyncReplyChannel<State>
    | ElectionTimeout
    | RequestVote of RequestVoteArguments * AsyncReplyChannel<RequestVoteResult>
    | AppendEntries of AppendEntriesArguments * AsyncReplyChannel<AppendEntriesResult>
    | RequestVoteResult of RequestVoteArguments * RequestVoteResult
    | AppendEntiresResult of AppendEntriesArguments * AppendEntriesResult

type RaftNode (startNewElectionTimeout, nodeId, otherNodes:IRemoteRaftNode array, initialState) =

    let sendRequestVoteToAllNodes request =
        Array.iter (fun (n:IRemoteRaftNode) -> n.RequestVote request) otherNodes

    let electionTimeout (agent:MailboxProcessor<Message>) state =
        sendRequestVoteToAllNodes { term = 1; candidateId = 1; lastLogIndex = 1; lastLogTerm = 1}
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

    member x.RequestVoteResult request result = 
        agent.Post(RequestVoteResult (request, result))

    member x.ElectionTimeout () = 
        agent.Post ElectionTimeout

    interface IRaftNode with
        member x.GetState () = agent.PostAndAsyncReply(GetState)
        member x.RequestVote request = agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))
        member x.AppendEntries request = agent.PostAndAsyncReply(fun rc -> AppendEntries (request, rc))
        member x.RequestVoteResult request result = agent.Post(RequestVoteResult (request, result))

module RaftAgentWrapper =

    let startNewElectionTimeout minElectionTimeout (agent:MailboxProcessor<Message>) = 
        Elections.startElectionTimeout (TimeSpan.FromMilliseconds minElectionTimeout) (fun () -> agent.Post ElectionTimeout)

    let defaultState = 
        { state = Follower; currentTerm = 0; votedFor = None; electionTimeout = None }

    let createAgent minElectionTimeout nodeId state =
        match state with
        | Some actualState ->
            RaftNode (startNewElectionTimeout minElectionTimeout, nodeId, Array.empty, actualState) :> IRaftNode
        | None ->
            RaftNode (startNewElectionTimeout minElectionTimeout, nodeId, Array.empty, defaultState) :> IRaftNode
