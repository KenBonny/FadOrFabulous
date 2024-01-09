module Drone.Api.Domain.Drone

type Make = string
type Model = string

[<CLIMutable>]
type Drone = {
    Id: int
    Make: Make
    Model: Model
}