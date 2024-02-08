module ``Collisions Test``

open Drone.Api.Domain.Drone
open Drone.Api.Features
open Drone.Shared.Domain.Drone
open Messages
open Xunit

[<Fact>]
let ``Detect collision`` () =
    let drone : Drone = {
        Id = 1
        Make = "DJI"
        Model = "Mavic 3 Pro"
    }
    let existingFlights : Flight list = [
        {
            Id = 1
            DroneId = 1
            Path = [
                TakeOff { Lat = 0; Long = 0 }
                Waypoint { Lat = 1; Long = 1 }
                Waypoint { Lat = 2; Long = 2 }
                Land { Lat = 3; Long = 3 }
            ]
        }
        {
            Id = 2
            DroneId = 1
            Path = [
                TakeOff { Lat = 1; Long = 2 }
                Waypoint { Lat = 2; Long = 2 }
                Land { Lat = 4; Long = 4 }
            ]
        }
    ]

    let newPath = [
        TakeOff { Lat = 0; Long = 1 }
        Waypoint { Lat = 1; Long = 1 }
        Waypoint { Lat = 2; Long = 4 }
        Land { Lat = 3; Long = 1 }
    ]

    let collision = RegisterFlight.validateFlight drone existingFlights newPath
    match collision with
    | FlightRejected reason -> Assert.Equal("Collision detected at 1::1", reason)
    | _ -> Assert.Fail("Should have been rejected")
    ()

[<Fact>]
let ``No collisions should accept the flight`` () =
    let drone : Drone = {
        Id = 1
        Make = "DJI"
        Model = "Mavic 3 Pro"
    }
    let existingFlights : Flight list = [
        {
            Id = 1
            DroneId = 1
            Path = [
                TakeOff { Lat = 0; Long = 0 }
                Waypoint { Lat = 1; Long = 1 }
                Waypoint { Lat = 2; Long = 2 }
                Land { Lat = 3; Long = 3 }
            ]
        }
        {
            Id = 2
            DroneId = 1
            Path = [
                TakeOff { Lat = 1; Long = 2 }
                Waypoint { Lat = 2; Long = 2 }
                Land { Lat = 4; Long = 4 }
            ]
        }
    ]

    let newPath = [
        TakeOff { Lat = 0; Long = 1 }
        Waypoint { Lat = 1; Long = 3 }
        Waypoint { Lat = 2; Long = 4 }
        Land { Lat = 3; Long = 1 }
    ]

    let collision = RegisterFlight.validateFlight drone existingFlights newPath
    match collision with
    | FlightRegistered _ -> ()
    | FlightRejected reason -> Assert.Fail("Should have been accepted: " + reason)
    ()