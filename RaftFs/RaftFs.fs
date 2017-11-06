namespace RaftFs

module RaftFs = 

    open System
    open System.IO
    open System.Net
    open System.Net.Sockets
    open System.Text

    type ElectionTimeout = {
        mutable LastReset : DateTime
        Timeout : TimeSpan
    }

    let extendTimeout election = 
        election.LastReset <- DateTime.UtcNow

    let startElectionTimeout timeout callback = 
        let election = {
            LastReset = DateTime.UtcNow;
            Timeout = timeout;
        }
        let rec waitForNextElection election = async {
            let msToElectionTimeout = int (election.LastReset + election.Timeout - DateTime.UtcNow).TotalMilliseconds
            if msToElectionTimeout <= 0 then
                callback()
            else
                do! Async.Sleep msToElectionTimeout
                do! waitForNextElection election
        }
        Async.Start(waitForNextElection election)
        election

    let readAllBytes (s : Stream) = 
        let ms = new MemoryStream()
        s.CopyTo(ms)
        ms.ToArray()

    let startTcpServer() = 
        let server = TcpListener(IPAddress.Parse("127.0.0.1"), 13000)
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

    let main argv =
        let timeout () = printfn "Timeout reached!"
        let election = startElectionTimeout (TimeSpan.FromSeconds (float 2)) timeout

        while true do
            Console.ReadLine() |> ignore
            extendTimeout election

        0
