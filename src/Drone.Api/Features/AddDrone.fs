module Drone.Api.Features.AddDrone

open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Microsoft.AspNetCore.Http
open Wolverine.Http

type CreateDrone = {
    Make: Make
    Model: string
}

type DroneCreated = {
    Drone: Drone
}

type CreatedDrone = DroneId

[<Tags("Drone")>]
[<WolverinePost("drone")>]
let createDrone (droneDto: CreateDrone) (context: DroneContext) =
    task {
        let drone = {
            Id = 0
            Make = droneDto.Make
            Model = Model droneDto.Model
        }
        context.Add drone |> ignore
        let! _ = context.SaveChangesAsync()
        return struct (Results.Created("/drones", drone.Id), { Drone = drone })
    }