module Drone.Api.Features.NotifySubscribersOfNewDrone

open Drone.Api.Features.AddDrone

type Handler () =
    member this.Handle (message: DroneCreated) =
        printfn $"Does it work with Type? drone registered: {message.Drone}"