module Drone.Api.Features.NotifySubscribersOfNewDrone

open Messages

type Handler () =
    member this.Handle (message: DroneMessage) =
        printfn $"Does it work with Type? drone registered: {message.Payload}"