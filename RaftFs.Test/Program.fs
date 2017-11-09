namespace RaftFs.Test

module Program =

    // Workaround for warnings, also makes it easier to debug tests
    [<EntryPoint>]
    let main argv =
        RaftAgentTest.``Election timeout for Follower``()
        0
