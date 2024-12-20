﻿module Drone.Api.Features.NotifySubscribersHandler

open System
open System.Threading.Tasks
open Drone.Api.Features.AddDrone
open Messages
open Microsoft.Extensions.Logging
open Wolverine.Attributes

[<WolverineHandler>]
let notifyWithDelay (message: DroneCreated) (logger: ILogger) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        logger.LogInformation("Does it work with attributes? drone registered: {Message}", message)
    }

let Handle (message: DroneCreated) =
    printfn $"Does it work with correct naming? drone registered: {message}"

// [<WolverineHandler>]
// let processSharedMessage (message: SharedMessage) =
//     printfn $"Shared message handled: {message}"
//
// [<WolverineHandler>]
// let flightRegistered (message: DiscriminatedUnionMessage<FlightRegistered>) =
//     match message.Payload with
//     | FlightRegistered flight -> printfn $"Flight registered: {flight.Id} for drone {flight.DroneId}"
//     | FlightRejected reason -> printfn $"Flight rejected: {reason}"