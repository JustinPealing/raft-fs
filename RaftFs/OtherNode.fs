namespace RaftFs

open System
open Messages
open Rpc

type NodeCommunicationMessage = 
    | Ping
    | RequestVote of RequestVoteArguments

type OtherNode (client:IRpcClient, raft:IRaftAgent) =

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

    interface IOtherNode with
        member x.RequestVote request = 
            agent.Post (RequestVote request)
