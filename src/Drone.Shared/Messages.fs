module Messages

[<Literal>]
let droneQueueName = "drone-messages"

type SharedMessage = {
  id: int
  text: string
}