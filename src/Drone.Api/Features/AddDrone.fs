module Drone.Api.Features.AddDrone

open Drone.Api.Database.DroneContext
open Drone.Shared.Domain.Drone
open Messages
open Microsoft.AspNetCore.Http
open Wolverine.Http

type CreateDrone = {
    Make: Make
    Model: string
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
        let msg: SharedMessage = { id = drone.Id; text = drone.Model }
        return struct (Results.Created("/drones", drone.Id), DroneCreated(drone), { id = drone.Id; text = drone.Model })
    }