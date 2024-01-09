module Program

#nowarn "20"
open System.IO
open Drone.Api.Database.DroneContext
open Drone.Api.WolverineOperationFilter
open Microsoft.AspNetCore.Builder
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen
open Swashbuckle.AspNetCore.SwaggerUI
open Wolverine
open Wolverine.Http

[<Literal>]
let exitCode = 0

let configureEF (options:DbContextOptionsBuilder) =
    options.UseInMemoryDatabase("DroneDb")
    ()

let configureSwaggerGen (options:SwaggerGenOptions) =
    options.SwaggerDoc("v1", OpenApiInfo(
        Version = "v1",
        Title = "Drone API",
        Description = "Drone API to track drones across the sky"
    ))
    options.OperationFilter<WolverineOperationFilter>()
    let xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"
    let path = Path.Combine(System.AppContext.BaseDirectory, xmlFile)
    options.IncludeXmlComments(path)
    ()

let configureSwaggerUi (options:SwaggerUIOptions) =
    options.ConfigObject.TryItOutEnabled <- true
    ()

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services
        .AddEndpointsApiExplorer()
        .AddDbContext<DroneContext>(configureEF)
        .AddSwaggerGen(configureSwaggerGen)
    builder.Host.UseWolverine()

    let app = builder.Build()

    app.UseHttpsRedirection()
    // app.UseAuthorization()
    app.MapWolverineEndpoints()
    app.UseSwagger().UseSwaggerUI(configureSwaggerUi)

    app.Run()

    exitCode