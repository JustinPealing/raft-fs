namespace RaftFs

open System
open System.Text
open RaftFs.Messages

module Serialization = 

    let Serialize req = 
        let str = req.success.ToString()
        Encoding.UTF8.GetBytes str

    let DeserializeAppendEntriesResult data = 
        let string = Encoding.UTF8.GetString data
        {success = bool.Parse string}
