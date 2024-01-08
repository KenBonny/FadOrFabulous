namespace Drone.Api
#nowarn "20"
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Program =
    [<Literal>]
    let exitCode = 0

let configureEF (options:DbContextOptionsBuilder) =
    options.UseInMemoryDatabase("DroneDb")
    ()

[<EntryPoint>]
let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()
        .AddDbContext<DroneContext>(configureEF)

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode