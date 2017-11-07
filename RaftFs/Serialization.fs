namespace RaftFs

open System
open System.Text
open RaftFs.Messages

module Serialization = 

    let SerializeAppendEntriesResult obj =
        let str = obj.success.ToString()
        Encoding.UTF8.GetBytes str

    let SerializeAppendEntiresArguments obj = 
        let str = obj.term.ToString()
        Encoding.UTF8.GetBytes str

    let DeserializeAppendEntriesResult data = 
        let string = Encoding.UTF8.GetString data
        {success = bool.Parse string}

    let DeserializeAppendEntriesArguments data = 
        let string = Encoding.UTF8.GetString data
        {term = Int32.Parse string}
