module Drone.Api.Features.RegisterFlight

open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Drone.Shared.Domain.Drone
open Messages
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Wolverine.Http

let collision first second =
    if first = second then
        Some first
    else
        None

let detectCollision newPath existingPath =
    match newPath, existingPath with
    | TakeOff coordinate, TakeOff existing -> collision coordinate existing
    | TakeOff _, Waypoint _ -> None
    | TakeOff coordinate, Land existing -> collision coordinate existing
    | Waypoint _, TakeOff _ -> None
    | Waypoint coordinate, Waypoint existing -> collision coordinate existing
    | Waypoint _, Land _ -> None
    | Land coordinate, TakeOff existing -> collision coordinate existing
    | Land _, Waypoint _ -> None
    | Land coordinate, Land existing -> collision coordinate existing

let rec detectCollisionAlongFlightPath newFlightPath existingFlightPath =
    match existingFlightPath with
    | [] -> None
    | existing :: restOfFlight ->
        newFlightPath
        |> List.map (detectCollision existing)
        |> List.choose id
        |> List.sort
        |> function
            | [] -> detectCollisionAlongFlightPath restOfFlight newFlightPath
            | first :: _ -> Some first

let rec validateFlight (drone: Drone) existingFlights newFlightPath =
    if obj.ReferenceEquals(drone, null) then
        FlightRejected($"Drone {drone.Id} not found")
    else
        let collision =
            existingFlights
            |> List.map _.Path
            |> List.map (detectCollisionAlongFlightPath newFlightPath)
            |> List.choose id
            |> List.tryHead
        match collision with
        | Some coordinate -> FlightRejected($"Collision detected at {coordinate.Lat}::{coordinate.Long}")
        | None -> FlightRegistered { Id = 0; DroneId = drone.Id; Path = newFlightPath }


/// <summary>Register a flight for a drone</summary>
/// <remarks>
/// [
///   {
///     "Case": "TakeOff",
///     "Fields": [
///       {
///         "Lat": 1,
///         "Long": 1
///       }
///     ]
///   },
///   {
///     "Case": "Waypoint",
///     "Fields": [
///       {
///         "Lat": 2,
///         "Long": 2
///       }
///     ]
///   },
///   {
///     "Case": "Land",
///     "Fields": [
///       {
///         "Lat": 5,
///         "Long": 2
///       }
///     ]
///   }
/// ]
/// </remarks>
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