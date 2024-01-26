module Drone.Api.Database.DroneContext

open Drone.Api.Domain.Drone
open Microsoft.EntityFrameworkCore
open Newtonsoft.Json

type DroneContext =
    inherit DbContext

    new() = { inherit DbContext() }
    new(options: DbContextOptions<DroneContext>) = { inherit DbContext(options) }

    [<DefaultValue>]
    val mutable private drones: DbSet<Drone>
    member this.Drones
        with get() = this.drones
        and set value = this.drones <- value

    [<DefaultValue>]
    val mutable private flights : DbSet<Flight>
    member this.Flights
        with get() = this.flights
        and set value = this.flights <- value

    override this.OnModelCreating (builder: ModelBuilder) =
        builder.Entity<Flight>().Property(_.Path).HasConversion(
            (fun path -> JsonConvert.SerializeObject path),
            (fun path -> JsonConvert.DeserializeObject<FlightPath list> path)
        ) |> ignore
        ()