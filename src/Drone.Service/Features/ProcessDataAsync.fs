module ProcessDataAsyncHandler

open Messages
open Wolverine.Attributes

let Handle (message: SharedMessage) =
    printfn $"Shared Message {message.id}: {message.text}"

// todo: message is not picked up
[<WolverineHandler>]
let flightRegistered (message: DiscriminatedUnionMessage<FlightRegistered>) =
    match message.Payload with
    | FlightRejected reason -> printfn $"Flight rejected because {reason}"
    | FlightRegistered flight -> printfn $"Flightpath added: {flight}"