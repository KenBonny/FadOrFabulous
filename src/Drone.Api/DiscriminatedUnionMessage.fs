module Drone.Api.DiscriminatedUnionMessage

type DiscriminatedUnionMessage<'a> = { Payload: 'a }

let toMessage payload = { Payload = payload }