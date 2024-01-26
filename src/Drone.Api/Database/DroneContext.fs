module Drone.Api.Database.DroneContext

open Drone.Api.Domain.Drone
open Microsoft.EntityFrameworkCore

type DroneContext =
    inherit DbContext

    new() = { inherit DbContext() }
    new(options: DbContextOptions<DroneContext>) = { inherit DbContext(options) }

    [<DefaultValue>]
    val mutable private drones: DbSet<Drone>
    member this.Drones
        with get() = this.drones
        and set value = this.drones <- value