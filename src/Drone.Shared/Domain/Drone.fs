module Drone.Shared.Domain.Drone

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