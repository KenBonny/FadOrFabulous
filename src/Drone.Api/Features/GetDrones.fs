module Drone.Api.Features.GetDrones

open System
open System.Linq
open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Wolverine.Http

type DroneDto = { Make: string; Model: Model }

let inline (&?) (value: 'a Nullable) defaultValue =
    if value.HasValue then value.Value else defaultValue

let retrievePage page pageSize =
    let page = (page &? 1) - 1
    let pageSize = pageSize &? 200
    page * pageSize, pageSize

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
    let page, pageSize = retrievePage page pageSize

    context.Drones
        .OrderBy(fun drone -> drone.Make)
        .ThenBy(fun drone -> drone.Model)
        .Skip(page)
        .Take(pageSize)
        .Select(fun drone ->
            { Make = drone.Make
              Model = drone.Model })
        .ToListAsync()