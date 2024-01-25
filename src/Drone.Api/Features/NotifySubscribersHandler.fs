module Drone.Api.Features.NotifySubscribersHandler

open System
open System.Threading.Tasks
open Messages
open Wolverine.Attributes

[<WolverineHandler>]
let notifyWithDelay (message: DroneEvents) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        printfn $"Does it work with attributes? drone registered: {message}"
    }

[<WolverineHandler>]
let processSharedMessage (message: SharedMessage) =
    printfn $"Shared message handled: {message}"

let Handle = function
    | DroneCreated drone -> printfn $"Does it work with correct naming? drone registered: {drone}"
    | DronesListed drones -> printfn $"Does it work with correct naming? drones list: {drones}"