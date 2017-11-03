module RaftFs

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text

let readAllBytes (s : Stream) = 
    let ms = new MemoryStream()
    s.CopyTo(ms)
    ms.ToArray()

[<EntryPoint>]
let main argv =
    let server = new TcpListener(IPAddress.Parse("127.0.0.1"), 13000)
    server.Start()

    let rec loop() = async {
        printfn "Waiting for connection..."
        let! client = server.AcceptTcpClientAsync() |> Async.AwaitTask

        printfn "Connection established"
        let stream = client.GetStream()
        let reader = new StreamReader(stream)

        let str = reader.ReadLine()
        printfn "%s" str

        return! loop()
    }

    Async.Start(loop())

    printfn "Press enter to exit"
    Console.ReadLine() |> ignore
    0
