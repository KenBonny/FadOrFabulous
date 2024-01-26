module Messages

open Drone.Shared.Domain.Drone

[<Literal>]
let droneQueueName = "drone-messages"
let createQueueName (t:System.Type) = $"{t.Namespace}.{t.Name}".Replace(".", "-")

type DiscriminatedUnionMessage<'a> = { Payload: 'a }

let toMessage payload = { Payload = payload }

type SharedMessage = {
  id: int
  text: string
}

type FlightRegistered =
    | FlightRejected of string
    | FlightRegistered of Flight