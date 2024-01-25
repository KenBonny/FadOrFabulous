module ProcessDataAsyncHandler

open Messages

let Handle (message: SharedMessage) =
    printfn $"Shared Message {message.id}: {message.text}"