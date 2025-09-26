module Drone.Api.Features.GetDrones

open System
open System.Linq
open Drone.Api.Database.DroneContext
open Drone.Api.Domain.Drone
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Wolverine.Http

type DroneDto = { Make: string; Model: Model }

let inline (&?) (value: 'a Nullable) defaultValue =
    if value.HasValue then value.Value else defaultValue

let retrievePage page pageSize =
    let page = (page &? 1) - 1
    let pageSize = pageSize &? 200
    page * pageSize, pageSize

/// <summary>
/// Get a list of drones
/// </summary>
/// <param name="page">The page to retrieve</param>
/// <param name="pageSize">The number of items per page</param>
/// <param name="context">The database connection</param>
/// <remarks>
/// Sample request:
///   GET /drones
///   [
///     {
///       "make": "DJI",
///       "model": "Mavic 3 Pro"
///     }
///   ]
/// </remarks>
[<Tags("Drone")>]
[<WolverineGet("drones")>]
let getDrones page pageSize (context: DroneContext) =
    let page, pageSize = retrievePage page pageSize
    context.Drones
        .OrderBy(fun drone -> drone.Make)
        .ThenBy(_.Model)
        .Skip(page)
        .Take(pageSize)
        .Select(fun drone ->
           { Make = drone.Make
             Model = drone.Model })
        .ToListAsync()


// open FsToolkit.ErrorHandling
// type Email = string
// type User = { Email: Email; Age: int }
// type InvalidUser =
//     | NotAnEmail of string
//     | TooYoung of int
//     | InvalidAge of string
// let tryParseEmailAddress (email: string) : Result<Email, InvalidUser> =
//     if email.Contains "@" then Ok email else Error (NotAnEmail email)
//
// let tryParseAge (age: string) : Result<int, InvalidUser> =
//     match Int32.TryParse age with
//     | true, age when age >= 18 -> Ok age
//     | true, age -> Error (TooYoung age)
//     | _ -> Error (InvalidAge age)
//
// let tryUserSignup email age =
//     result { 
//         let! email = tryParseEmailAddress email
//         let! age = tryParseAge age
//         return { Email = email; Age = age }
//     }