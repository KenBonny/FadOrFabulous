module Drone.Api.Features.GetDrones

open System
open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Microsoft.AspNetCore.Http
open Wolverine.Http

type DroneDto = { Make: string; Model: Model }

let inline (&?) defaultValue (value: 'a Nullable) =
    if value.HasValue then value.Value else defaultValue
let defaultPageSize = (&?) 200

let skip' page pageSize =
    let page = (1 &? page) - 1
    let pageSize = defaultPageSize pageSize
    page * pageSize

/// <summary>
/// Get a list of drones
/// </summary>
/// <param name="page">The page to retrieve</param>
/// <param name="pageSize">The number of items per page</param>
/// <param name="context">The database connection</param>
/// <remarks>
/// Sample request:
///   GET /drones
///   [
///     {
///       "make": "DJI",
///       "model": "Mavic 3 Pro"
///     }
///   ]
/// </remarks>
[<Tags("Drone")>]
[<WolverineGet("drones")>]
let getDrones page pageSize (context: DroneContext) =
    query {
        for drone in context.Drones do
        sortBy drone.Make
        thenBy drone.Model
        skip (skip' page pageSize)
        take (defaultPageSize pageSize)
        select
            { Make = drone.Make
              Model = drone.Model }
    }
    |> Seq.toList