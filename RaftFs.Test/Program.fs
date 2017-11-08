namespace RaftFs.Test

module Program =
    // Workaround for warnings
    [<EntryPoint>]
    let main argv =
        RpcTest.``Make RPC``()
        0
