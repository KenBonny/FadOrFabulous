module Drone.Api.Features.RegisterFlight

open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Drone.Api.Features.SaveFlight
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
    | TakeOff coordinate, Land existing -> collision coordinate existing
    | Waypoint coordinate, Waypoint existing -> collision coordinate existing
    | Land coordinate, TakeOff existing -> collision coordinate existing
    | Land coordinate, Land existing -> collision coordinate existing
    | TakeOff _, Waypoint _ -> None
    | Waypoint _, TakeOff _ -> None
    | Waypoint _, Land _ -> None
    | Land _, Waypoint _ -> None

let detectCollisionAlongFlightPath (newFlightPath: FlightPath list) (existingFlightPath: FlightPath list) =
    let equal = List.min [newFlightPath.Length; existingFlightPath.Length] |> List.take
    List.map2 detectCollision (equal newFlightPath) (equal existingFlightPath)
    |> List.choose id // remove None

let validateFlight (drone: Drone) existingFlights newFlightPath =
    if obj.ReferenceEquals(drone, null) then
        FlightRejected($"Drone {drone.Id} not found")
    else
        let detectCollisions flight = detectCollisionAlongFlightPath newFlightPath flight.Path
        existingFlights
        |> List.collect detectCollisions
        |> List.sort
        |> List.tryHead
        |> function
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
        let existingFlights = List.ofSeq existingFlights
        let flightRegistered = validateFlight drone existingFlights trajectory
        let message = toMessage flightRegistered
        return
            match flightRegistered with
            | FlightRejected reason -> struct (Results.Problem(reason), message, SaveFlight(None))
            | FlightRegistered flight -> struct (Results.Ok(flight.Id), message, SaveFlight(Some flight))
    }