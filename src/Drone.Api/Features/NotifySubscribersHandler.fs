module Drone.Api.Features.NotifySubscribersHandler

open System
open System.Threading.Tasks
open Drone.Api.Features.AddDrone
open Wolverine.Attributes

[<WolverineHandler>]
let notifyWithDelay (message: DroneCreated) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        printfn $"Does it work with attributes? drone registered: {message.Drone}"
    }

let Handle (message: DroneCreated) =
    printfn $"Does it work with correct naming? drone registered: {message.Drone}"