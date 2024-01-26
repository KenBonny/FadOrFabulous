module Drone.Api.Domain.Drone

type Make = string
type Model = string

[<CLIMutable>]
type Drone = {
    Id: int
    Make: Make
    Model: Model
}

type Coordinate = { Lat: int; Long: int }

type FlightPath =
    | TakeOff of Coordinate
    | Waypoint of Coordinate
    | Land of Coordinate

[<CLIMutable>]
type Flight = {
    Id: int
    DroneId: int
    Path: FlightPath list
}