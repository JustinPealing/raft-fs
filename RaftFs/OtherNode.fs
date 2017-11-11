namespace RaftFs

open System
open Messages
open Rpc

type NodeCommunicationMessage = 
    | Ping
    | RequestVote of RequestVoteArguments * AsyncReplyChannel<RequestVoteResult> 

type OtherNode(client:IRpcClient) =

    let agent = MailboxProcessor<NodeCommunicationMessage>.Start(fun inbox -> 
        let rec messageLoop() = async {
            let! message = inbox.Receive()

            inbox.Post Ping
            match message with
            | Ping -> ()
            | RequestVote (request, rc) -> 
                let! response = client.Send request
                ()
            // | AppendEntries (request, rc) ->
            //     let! response = client.AppendEntries request
            //     ()

            return! messageLoop()
        }
        messageLoop()
    )

    member this.RequestVote request =
        agent.PostAndAsyncReply(fun rc -> RequestVote (request, rc))
