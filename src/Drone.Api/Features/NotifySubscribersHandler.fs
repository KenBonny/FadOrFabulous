module Drone.Api.Features.NotifySubscribersHandler

open System
open System.Threading.Tasks
open Drone.Api.Features.AddDrone
open Messages
open Wolverine.Attributes

[<WolverineHandler>]
let notifyWithDelay (message: DroneCreated) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        printfn $"Does it work with attributes? drone registered: {message}"
    }

[<WolverineHandler>]
let processSharedMessage (message: SharedMessage) =
    printfn $"Shared message handled: {message}"

let Handle (message: DroneCreated) =
    printfn $"Does it work with correct naming? drone registered: {message}"