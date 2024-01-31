module Drone.Service.Program

#nowarn "20"
open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Wolverine
open Wolverine.RabbitMQ

let env =
    let value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    if String.IsNullOrEmpty value then "Development" else value

let config =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional = true)
        .AddJsonFile($"appsettings.{env}.json", optional = true)
        .AddEnvironmentVariables()
        .Build()

let configureWolverine (config:IConfiguration) (options:WolverineOptions) =
    options.ServiceName <- "Drone.Service"
    options
        .UseRabbitMq(Uri(config.GetConnectionString("RabbitMQ")))
        .AutoProvision()
        .EnableWolverineControlQueues()
        .UseConventionalRouting()
    ()

[<EntryPoint>]
let main args =
    let mutable exitCode = 0
    task {
        let builder = Host.CreateDefaultBuilder(args)
        let! result =
            builder
                .UseWolverine(configureWolverine config)
                .RunWolverineAsync(args)
        exitCode = result
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    exitCode