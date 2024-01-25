module Drone.Api.Features.NotifySubscribersHandler

open System
open System.Threading.Tasks
open Messages
open Wolverine.Attributes

[<WolverineHandler>]
let notifyWithDelay (message: DroneMessage) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        printfn $"Does it work with attributes? drone registered: {message.Payload}"
    }

[<WolverineHandler>]
let processSharedMessage (message: SharedMessage) =
    printfn $"Shared message handled: {message}"

let Handle (message:DroneMessage) =
    match message.Payload with
    | DroneCreated drone -> printfn $"Does it work with correct naming? drone registered: {drone}"
    | DronesListed drones -> printfn $"Does it work with correct naming? drones list: {drones}"