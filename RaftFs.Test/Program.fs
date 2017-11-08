namespace RaftFs.Test

module Program =

    // Workaround for warnings, also makes it easier to debug tests
    [<EntryPoint>]
    let main argv =
        RaftAgentTest.``ElectionTimeout via MailboxProcessor``()
        0
