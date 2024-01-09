module Drone.Api.Features.GetDrones

open System
open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Wolverine.Http

type DroneDto = { Make: string; Model: Model }

let inline (&?) defaultValue (value: 'a Nullable) =
    if value.HasValue then value.Value else defaultValue
let defaultPageSize = (&?) 200

let skip' page pageSize =
    let page = (1 &? page) - 1
    let pageSize = defaultPageSize pageSize
    page * pageSize

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