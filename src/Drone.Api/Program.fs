module Program

#nowarn "20"
open Drone.Api.Database.DroneContext
open Microsoft.AspNetCore.Builder
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
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
        .AddSwaggerGen(fun c ->
            c.SwaggerDoc("v1", OpenApiInfo(
                Version = "v1",
                Title = "Drone API",
                Description = "Drone API to track drones across the sky"
            )))
    builder.Host.UseWolverine()

    let app = builder.Build()

    app.UseHttpsRedirection()
    // app.UseAuthorization()
    app.MapWolverineEndpoints()
    app.UseSwagger().UseSwaggerUI(fun o -> o.ConfigObject.TryItOutEnabled <- true)

    app.Run()

    exitCode