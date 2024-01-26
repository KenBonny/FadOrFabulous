module Drone.Api.Features.RegisterFlight

open Drone.Api.Database.DroneContext
open Drone.Shared.Domain.Drone
open Messages
open Microsoft.AspNetCore.Http
open Wolverine.Http

[<Tags("Drone")>]
[<WolverinePost("drone/{droneId}/flight")>]
let registerFlight (droneId: int) (trajectory: FlightPath list) (db: DroneContext) =
    task {
        let! drone = db.Drones.FindAsync(droneId)
        if obj.ReferenceEquals(drone, null) then
            let reason = $"Drone {droneId} not found"
            return struct (Results.NotFound(reason), FlightRejected(reason) |> toMessage)
        else
            let flight = { Id = 0; DroneId = droneId; Path = trajectory }
            db.Flights.Add(flight) |> ignore
            let! _ = db.SaveChangesAsync()
            return struct (Results.Ok(flight.Id), FlightRegistered(flight) |> toMessage)
    }


[<Tags("Drone")>]
[<WolverineGet("drone/{droneId}/flight")>]
let getFlight droneId =
    {
        Id = 1
        DroneId = droneId
        Path = [
            TakeOff { Lat = 1; Long = 1}
            Waypoint { Lat = 2; Long = 2}
            Waypoint { Lat = 4; Long = 8}
            Land { Lat = 5; Long = 2}
        ]
    }