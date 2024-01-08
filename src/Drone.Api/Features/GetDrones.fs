module Drone.Api.Features.GetDrones

open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Wolverine.Http

type Request = Page

type DroneDto = { Make: string; Model: Model }

[<WolverineGet("drones")>]
let getDrones (request: Request) (context: DroneContext) =
    query {
        for drone in context.Drones do
        skip (request.PageSize * (request.PageNumber - 1))
        take request.PageSize
        select {
            Make = drone.Make
            Model = drone.Model
        }
    } |> Seq.toList