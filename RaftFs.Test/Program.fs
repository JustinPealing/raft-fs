namespace RaftFs.Test

module Program =

    // Workaround for warnings, also makes it easier to debug tests
    [<EntryPoint>]
    let main argv =
        RaftNodeTest.``Election timeout for Follower``()
        0
