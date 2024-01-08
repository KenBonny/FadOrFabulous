module Program

#nowarn "20"
open Drone.Api.Database.DroneContext
open Drone.Api.Features
open Microsoft.AspNetCore.Builder
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Wolverine
open Wolverine.Http

[<Literal>]
let exitCode = 0

let configureEF (options:DbContextOptionsBuilder) =
    options.UseInMemoryDatabase("DroneDb")
    ()

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services
        .AddEndpointsApiExplorer()
        .AddDbContext<DroneContext>(configureEF)
    builder.Host.UseWolverine()

    let app = builder.Build()

    app.UseHttpsRedirection()
    // app.UseAuthorization()
    app.MapWolverineEndpoints()

    app.Run()

    exitCode