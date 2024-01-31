module Drone.Api.Features.RegisterFlight

open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Drone.Shared.Domain.Drone
open Messages
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Wolverine.Http

type Collision =
    | NoCollision
    | Collision of Coordinate

let collision first second =
    if first = second then
        Collision first
    else
        NoCollision

let detectCollision' newPath existingPath =
    let detectTakeOffOrLandingCollision coordinate =
        match existingPath with
        | TakeOff existingCoordinate -> collision coordinate existingCoordinate
        | Waypoint _ -> NoCollision
        | Land existingCoordinate -> collision coordinate existingCoordinate
    let detectWaypointCollision coordinate =
        match existingPath with
        | TakeOff _ -> NoCollision
        | Waypoint existingCoordinate -> collision coordinate existingCoordinate
        | Land _ -> NoCollision

    match newPath with
    | TakeOff coordinate -> detectTakeOffOrLandingCollision coordinate
    | Waypoint coordinate -> detectWaypointCollision coordinate
    | Land coordinate -> detectTakeOffOrLandingCollision coordinate

let rec detectCollision newFlightPath existingFlightPath =
    match existingFlightPath with
    | [] -> NoCollision
    | existing :: rest ->
        let possibleCollision =
            newFlightPath
            |> List.map (detectCollision' existing)
            |> List.tryFind (function | Collision _ -> true | _ -> false)
        match possibleCollision with
        | Some collision -> collision
        | None -> detectCollision rest newFlightPath

let rec validateFlight (drone: Drone) existingFlights newFlightPath =
    if obj.ReferenceEquals(drone, null) then
        FlightRejected($"Drone {drone.Id} not found")
    else
        let collision =
            existingFlights
            |> List.map _.Path
            |> List.map (detectCollision newFlightPath)
            |> List.choose (function | Collision coordinate -> Some coordinate | _ -> None)
            |> List.tryHead
        match collision with
        | Some coordinate -> FlightRejected($"Collision detected at {coordinate.Lat}::{coordinate.Long}")
        | None -> FlightRegistered { Id = 0; DroneId = drone.Id; Path = newFlightPath }

[<Tags("Drone")>]
[<WolverinePost("drone/{droneId}/flight")>]
let registerFlight (droneId: int) (trajectory: FlightPath list) (db: DroneContext) =
    task {
        let! drone = db.Drones.FindAsync(droneId)
        let! existingFlights = db.Flights.ToListAsync()
        let existingFlights = existingFlights |> List.ofSeq
        let flightRegistered = validateFlight drone existingFlights trajectory
        let message = flightRegistered |> toMessage
        let result =
            match flightRegistered with
                | FlightRejected reason -> struct (Results.NotFound(reason), message)
                | FlightRegistered flight ->
                    db.Flights.Add(flight) |> ignore
                    struct (Results.Ok(flight.Id), message)

        let! _ = db.SaveChangesAsync()
        return result
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