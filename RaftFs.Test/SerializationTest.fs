namespace RaftFs.Test

open NUnit.Framework
open RaftFs
open RaftFs.Messages

module SerializationTest = 
    /// <summary>
    /// The Serialize functions all need to return a byte[] that can be sent down the wire.
    /// </summary>
    [<Test>]
    let ``Serialized AppendEntriesResult should be byte[]``() =
        let response = {success = false}
        let data = Serialization.SerializeAppendEntriesResult response
        Assert.That(data, Is.TypeOf<byte[]>())

    [<Test>]
    let ``Serialized AppendEntriesArguments should be byte[]``() =
        let response = {term = 7}
        let data = Serialization.SerializeAppendEntiresArguments response
        Assert.That(data, Is.TypeOf<byte[]>())
    
    /// <summary>
    /// Deserializing an object needs to return an object equivalent to the serialized one.
    /// </summary>
    [<Test>]
    let ``Serialize and Deserialize AppendEntriesResult``() = 
        let response = {success = false}
        let data = Serialization.DeserializeAppendEntriesResult (Serialization.SerializeAppendEntriesResult response)
        Assert.That(data, Is.EqualTo(response))

    [<Test>]
    let ``Serialize and Deserialize AppendEntriesArguments``() = 
        let response = {term = 17}
        let data = Serialization.DeserializeAppendEntriesArguments (Serialization.SerializeAppendEntiresArguments response)
        Assert.That(data, Is.EqualTo(response))
