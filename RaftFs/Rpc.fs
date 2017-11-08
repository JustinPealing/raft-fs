namespace RaftFs

open System
open System.IO
open System.Net
open System.Net.Sockets
open Messages
open ProtoBuf

module Rpc = 

    type Stream with
        member stream.ReadExactlyAsync count = 
            let buffer : byte[] = Array.zeroCreate count
            let rec read offset = async {
                let! n = stream.ReadAsync(buffer, offset, count - offset) |> Async.AwaitTask
                if offset + n = count then return buffer
                else return! read (offset + n)
            }
            read 0

    type IRpcClient =
        inherit IDisposable
        abstract member AppendEntries : AppendEntriesArguments -> Async<AppendEntriesResult>
        abstract member RequestVote : RequestVoteArguments -> Async<RequestVoteResult>

    let serialize obj =
        use stream = new MemoryStream()
        Serializer.Serialize(stream, obj)
        stream.ToArray()

    let deserialize<'T> (data:byte[]) =
        use stream = new MemoryStream(data)
        Serializer.Deserialize<'T>(stream)

    let CreateServer port appendEntries requestVote =
        let server = TcpListener(IPAddress.Parse("127.0.0.1"), port)
        server.Start()

        let rec acceptClientLoop() = async {
            let! client = server.AcceptTcpClientAsync() |> Async.AwaitTask
            let stream = client.GetStream()

            let! header = stream.ReadExactlyAsync 4
            let count = BitConverter.ToInt32(header, 0)

            let! body = stream.ReadExactlyAsync count
            let request = deserialize body

            let response = appendEntries request
            let responseBody = serialize response

            let responseHeader = BitConverter.GetBytes responseBody.Length
            do! stream.WriteAsync(responseHeader, 0, 4) |> Async.AwaitTask
            do! stream.WriteAsync(responseBody, 0, responseBody.Length) |> Async.AwaitTask
            do! stream.FlushAsync() |> Async.AwaitTask

            return! acceptClientLoop()
        }
        acceptClientLoop() |> Async.Start

        {new IDisposable with
            member this.Dispose() = server.Stop()}

    let CreateClient (host:string) (port:int) =

        let rpc request = async {
            let client = new TcpClient()
            do! client.ConnectAsync(host, port) |> Async.AwaitTask
            use stream = client.GetStream()
            
            let requestBody = serialize request
            let requestHeader = BitConverter.GetBytes requestBody.Length
            do! stream.WriteAsync(requestHeader, 0, 4) |> Async.AwaitTask
            do! stream.WriteAsync(requestBody, 0, requestBody.Length) |> Async.AwaitTask
            do! stream.FlushAsync() |> Async.AwaitTask

            let! responseHeader = stream.ReadExactlyAsync 4
            let count = BitConverter.ToInt32(responseHeader, 0)

            let! responseBody = stream.ReadExactlyAsync count
            return deserialize responseBody
        }

        {new IRpcClient with
            member this.Dispose() = ()
            member this.AppendEntries req = rpc req
            member this.RequestVote req = rpc req }
