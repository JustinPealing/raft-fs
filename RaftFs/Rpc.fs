namespace RaftFs

open System
open System.IO
open System.Net
open System.Net.Sockets
open Messages

module Rpc = 

    type IRpcClient =
        inherit IDisposable
        abstract member AppendEntries : AppendEntriesArguments -> Async<AppendEntriesResult>

    let CreateServer port appendEntries =
        let server = TcpListener(IPAddress.Parse("127.0.0.1"), port)
        server.Start()

        let rec loop() = async {
            let! client = server.AcceptTcpClientAsync() |> Async.AwaitTask
            let stream = client.GetStream()
            let reader = new StreamReader(stream)

            let! body = reader.ReadLineAsync() |> Async.AwaitTask
            let req = {
                term = int(body)
            }
            let resp = appendEntries req

            use writer = new StreamWriter(stream)
            do! writer.WriteLineAsync(resp.success.ToString()) |> Async.AwaitTask
            return! loop()
        }

        {new IDisposable with
            member this.Dispose() = server.Stop()}

    let CreateClient (host:string) (port:int) =

        let appendEntires (req:AppendEntriesArguments) = async {
            let client = new TcpClient()
            do! client.ConnectAsync(host, port) |> Async.AwaitTask
            use stream = client.GetStream()

            let writer = new StreamWriter(stream)
            do! writer.WriteLineAsync(req.term.ToString()) |> Async.AwaitTask
            do! writer.FlushAsync() |> Async.AwaitTask

            let reader = new StreamReader(stream)
            let! response = reader.ReadLineAsync() |> Async.AwaitTask
            
            return {success = bool.Parse(response)}
        }

        {new IRpcClient with
            member this.Dispose() = ()
            member this.AppendEntries req = appendEntires req }
