module Drone.Api.Features.SaveFlight

open Drone.Api.Database.DroneContext
open Drone.Shared.Domain.Drone
open Wolverine

type SaveFlight(flight: Flight option) =
    interface ISideEffect

    member this.ExecuteAsync (ctx: DroneContext) token =
        task {
            match flight with
            | None -> ()
            | Some flight ->
                ctx.Flights.Add flight |> ignore
                let! _ = ctx.SaveChangesAsync(token)
                ()
        }