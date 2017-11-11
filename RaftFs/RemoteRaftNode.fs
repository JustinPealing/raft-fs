namespace RaftFs

open System
open Messages
open Rpc

type NodeCommunicationMessage = 
    | Ping
    | RequestVote of RequestVoteArguments

type RemoteRaftNode (client:IRpcClient, raft:IRaftNode) =

    let agent = MailboxProcessor<NodeCommunicationMessage>.Start(fun inbox -> 
        let rec messageLoop() = async {
            let! message = inbox.Receive()

            inbox.Post Ping
            match message with
            | Ping -> ()
            | RequestVote request -> 
                let! result = client.Send request
                raft.RequestVoteResult request result
            // | AppendEntries (request, rc) ->
            //     let! response = client.AppendEntries request
            //     ()

            return! messageLoop()
        }
        messageLoop()
    )

    interface IRemoteRaftNode with
        member x.RequestVote request = 
            agent.Post (RequestVote request)
