module Messages

open Drone.Shared.Domain.Drone

[<Literal>]
let droneQueueName = "drone-messages"
let createQueueName (t:System.Type) = $"{t.Namespace}.{t.Name}".Replace(".", "-")

type SharedMessage = {
  id: int
  text: string
}

type DroneEvents =
    | DroneCreated of Drone
    | DronesListed of Drone list