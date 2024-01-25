module Messages

[<Literal>]
let droneQueueName = "drone-messages"
let createQueueName (t:System.Type) = $"{t.Namespace}.{t.Name}".Replace(".", "-")

type SharedMessage = {
  id: int
  text: string
}