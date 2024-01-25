module Drone.Api.Features.GetDrones

open System
open System.Linq
open Drone.Api.Database.DroneContext
open Drone.Shared.Domain.Drone
open Messages
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
    task {
        let page, pageSize = retrievePage page pageSize

        let! drones =
            context.Drones
                .OrderBy(fun drone -> drone.Make)
                .ThenBy(fun drone -> drone.Model)
                .Skip(page)
                .Take(pageSize)
                .ToListAsync()
        let dtos = drones.Select(
            fun drone ->
                { Make = drone.Make
                  Model = drone.Model })
                    .ToList()
        return struct (dtos, DronesListed(drones |> List.ofSeq) |> toMessage)
    }