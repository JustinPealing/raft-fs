namespace RaftFs

open System
open System.IO
open System.Net
open System.Net.Sockets
open Messages

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

    let CreateServer port appendEntries =
        let server = TcpListener(IPAddress.Parse("127.0.0.1"), port)
        server.Start()

        let rec acceptClientLoop() = async {
            let! client = server.AcceptTcpClientAsync() |> Async.AwaitTask
            let stream = client.GetStream()

            let! header = stream.ReadExactlyAsync 4
            let count = BitConverter.ToInt32(header, 0)

            let! body = stream.ReadExactlyAsync count
            let request = Serialization.DeserializeAppendEntriesArguments body

            let response = appendEntries request
            let responseBody = Serialization.SerializeAppendEntriesResult response

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

        let appendEntires (request:AppendEntriesArguments) = async {
            let client = new TcpClient()
            do! client.ConnectAsync(host, port) |> Async.AwaitTask
            use stream = client.GetStream()
            
            let requestBody = Serialization.SerializeAppendEntiresArguments request
            let requestHeader = BitConverter.GetBytes requestBody.Length
            do! stream.WriteAsync(requestHeader, 0, 4) |> Async.AwaitTask
            do! stream.WriteAsync(requestBody, 0, requestBody.Length) |> Async.AwaitTask
            do! stream.FlushAsync() |> Async.AwaitTask

            let! responseHeader = stream.ReadExactlyAsync 4
            let count = BitConverter.ToInt32(responseHeader, 0)

            let! responseBody = stream.ReadExactlyAsync count
            return Serialization.DeserializeAppendEntriesResult responseBody
        }

        {new IRpcClient with
            member this.Dispose() = ()
            member this.AppendEntries req = appendEntires req }
