# Functional programming in F#: Fad or Fabulous?

## Overview
- Who is this for?
- The technology used
  - F#
    - let keyword
    - ordering is important
    - piping
  - Wolverine
    - csharp framework
    - messages are central
  - Entity Framework
- Setting up the API
- Getting drone data
  - functions
  - lambda functions
  - simplified types
- Entity Framework setup
- Creating a new drone
  - computational expressions: task, others such as http and sql
  - let! and do!
- Handling a message
- registering a flight
  - discriminated unions
  - pipe operator
  - pattern matching
- Side effects
- testing
  - xunit
  - readable names for tests
  - DU's make testing easier
- processing messages in the background
  - rabbitmq or azure service bus
  - simple types can be used as messages
  - du messages need to be wrapped in a generic message type (or envelope)
  - discriminated unions and serialisation

## Who is this for?
Welcome to this talk on F# and Wolverine. I'm going to be telling you about how I used F# and Wolverine to build a drone tracking system. It's a very simple example system so I'm hoping that it will be easy to follow along. I hope that I can convey that F# is not as difficult as it may seem and that it's a very viable option for building systems.

What I'm not going to do is go into every minute detail of F#. There are a lot of introduction to F# articles out there, so I'm going to focus more on the more useful patterns and practices that make F# such a joy to use. The code I write here should be easy to understand, even if you've never seen F# before.

## The technology used
Well, first off there is F#. It's a functional dotnet language. As you follow along, you might see where C# gets its inspiration from. F# is a very powerful language and it can also be very simple. It's a very good language for writing code that is easy to understand and maintain. Of course, such as with any language, you can write difficult to understand code in F# too.

I'm pairing it with Wolverine. Wolverine is a framework that makes it easy to process messages. It can be seen as an alternative to NServiceBus or MassTransit. It's a very simple framework that makes it easy to process messages. It makes use of static functions a lot and F#'s functions compile to static functions, so the two pair quite well. I've also chosen it to show that interop between F# and C# is quite easy. Things that are written in one, can easily be used in the other.

Lastly, the major technology used is Entity Framework. This is mainly done to show that F# and EF work well together. Support in C# is better, but F# is still good.

## Setting up the API

Setting the API up is a very simple thing as Wolverine extends the ASP.NET framework. 

```fsharp
[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services
        .AddEndpointsApiExplorer()
        .AddDbContext<DroneContext>(configureEF)
        .AddSwaggerGen(configureSwaggerGen)
    builder.Host.UseWolverine(configureWolverine builder.Configuration)

    let app = builder.Build()

    app.UseHttpsRedirection()
    // app.UseAuthorization()
    app.MapWolverineEndpoints(fun o -> o.UseNewtonsoftJsonForSerialization())
    app.UseSwagger().UseSwaggerUI(configureSwaggerUi)

    app.Run()

    exitCode
```

In accordance to the Wolverine documentation, I'm adding the `MapWolverineEndpoints` which will find all the Wolverine endpoints. Defining endpoints will be discussed momentarily. Out of the box, Wolverine uses `System.Text.Json` for serialisation. I've chosen to use `Newtonsoft.Json` as it has better support for discriminated unions. I'll get more into that later.

What I want to focus on here is the `UseWolverine` function. This takes a lambda function that passes a `WolverineOptions` object and returns void. Yet, here we se a `configureWolverine` function that _"only"_ takes configuration. This is partial application of a function. You can read how it works in the [F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/partial-application/). It allows me to create another function from an already existing function by providing the first parameters. F# is full of nice little tricks like this.

## Getting drone data
The most simple thing that I can start with is retrieving drone data. This will take the form of an http endpoint that can be called. I'm going to use a simple function to do this. With the `let` keyword, I can define both functions as well as values, for more information check the [official docs](https://fsharp.org/docs/) and the [cheat sheet](https://github.com/fsprojects/fsharp-cheatsheet/blob/master/docs/fsharp-cheatsheet.md) or [F# for Fun and Profit](https://fsharpforfunandprofit.com/posts/fsharp-in-60-seconds/).

```fsharp
type Model = string
type DroneDto = { Make: string; Model: Model }
[<Tags("Drone")>]
[<WolverineGet("drones")>]
let getDrones page pageSize (context: DroneContext) =
    let page, pageSize = retrievePage page pageSize
    context.Drones
            .OrderBy(fun drone -> drone.Make)
            .ThenBy(fun drone -> drone.Model)
            .Skip(page)
            .Take(pageSize)
            .Select(fun drone ->
               { Make = drone.Make
                 Model = drone.Model })
            .ToListAsync()
```

After the name, we can add parameters and injectable dependencies. `page` and `pageSize` are query parameters that Wolverine will try to get from the request. Notice that their types are inferred by their usage. This inferring is because the type system is so strict, it can know in a lot of the cases what the type of these parameters is. In this case, it is because the `retrievePage` function makes that clear. The `context` parameter is a dependency that is injected by Wolverine. I specify that it's a `DroneContext` as this makes autocomplete a lot easier. F# could probably infer the type of `context` as well, but it is a lot more difficult for the type provider if it needs to infer this based on the properties used.

The `retrievePage` function is a simple function that takes two parameters and returns a tuple. Notice that I can assign the values of `page` and `pageSize` again. This is not the same as mutating the values as the original values would still be the same if they were referenced in an earlier part of the code.

Creating lambda or anonymous functions is also very easy. I use the `fun` keyword to create lambda functions for the ordering and selection. This is a very powerful feature of F# and it's used a lot. It's also a very simple way to create a function. The `fun` keyword is followed by the parameters and then the body of the function.

In the selection, the type is inferred again as the `DroneDto` record. A simple type like this is equivalent to a C# record and it's very simple to create. Also note that I can create aliases for any type that already exists. Instead of saying that a `Model` is a string, I can say that it's a `Model` type. This does not create warnings as an alias is just that, an alias. It's not a new type. Later, I'll show how to protect yourself better.

A last point to note is the strange notation of the attributes `[<Tags>]` and `[<WolverineGet>]`. These are attributes that Wolverine uses to know how to route the request. The `Tags` attribute is used to group endpoints together. The reason that it's different from C# is that F# has collection initialisers for a very long time. `[1; 2; 3]` creates an F# list of `int`s and `[| 1; 2; 3 |]` creates an array. I'm again referring to the docs, the cheat sheet and F# for Fun and Profit for more information on collections in F#. Let me just add that I find a bit confusing that almost every programming language uses square brackets for arrays and F# uses them for lists. This is because the F# list is much more used and thus got the easier notation. That does not make it any less confusing.

## Entity Framework setup

Setting up Entity Framework is quite similar to C#. Since F# doesn't have the same auto properties that C# has, it does take a bit more code to  create a `DbSet<>`. Nothing that can't be easily solved with a template though.

One thing that needs to added is that each type that is an Entity Framework table representation needs the attribute `[CLIMutable]`. This allows the CLR to mutate the properties, while in code they would need to be marked as `mutable`. This is where you see that EF is mainly meant for C#. I would look to a more F# friendly ORM if I were to use F# as my language of choice. There are plenty of good candidates out there.

```fsharp
type DroneContext =
    inherit DbContext

    new() = { inherit DbContext() }
    new(options: DbContextOptions<DroneContext>) = { inherit DbContext(options) }

    [<DefaultValue>]
    val mutable private drones: DbSet<Drone>
    member this.Drones
        with get() = this.drones
        and set value = this.drones <- value
```

## Creating a new drone
Now what would retrieving drone functionality be if there was no way of creating one.

```fsharp
type CreateDrone = {
    Make: Make
    Model: string
}
type DroneCreated = { drone: Drone }
[<Tags("Drone")>]
[<WolverinePost("drone")>]
let createDrone (droneDto: CreateDrone) (context: DroneContext) =
    task {
        let drone = {
            Id = 0
            Make = droneDto.Make
            Model = Model droneDto.Model
        }
        context.Add drone |> ignore
        let! _ = context.SaveChangesAsync()
        let droneCreated = { drone = drone }
        return struct (Results.Created("/drones", drone.Id), droneCreated)
    }
```

As the Wolverine concepts of the body deserialisation and `context` injection are the same as the get functionality, I'm going to focus on the more interesting parts in this function: computational expressions and the piping operator `|>`.

Let's start with the most simple of the two: the piping operator. This operator is used to pipe the result of one function into the next. If I did not mention it before, position is very important in F#. From the order of files, over declarations within a file all the way to the order of parameters in a function. Concepts can only be used after they are declared. I cannot make use of the type `CreateDrone` before it is declared. This means circular references cannot happen, it also ensures that you cannot use anything before it is declared. No jumping around, wondering where something is declared. It's always before it's used.

Back to the piping operator. The piping operator takes the output of the preceding function and passes it as the last parameter to the next function. So also the order of parameters is paramount. 🥁 The piping operator or `|>` is used a lot and makes code very readable. Here it is used to ignore the return value of the `.Add` function. You'll see much more of it later.

Now to the slightly more complex, but immensely powerful, computational expression. First of all, a computational expression is a way to write code that chains or-else operations. This is very useful for asynchronous code. The `task` keyword is a computation expression that is used to write asynchronous code. The `let!` keyword is used to await the result of an asynchronous operation. In regular functions, the last line returns the value of the function, here the specific `return` keyword needs to be used to return the result of the computation. The `return!` keyword is used to return the result of another computation. In C# this would be the equivalent of `return await someTask`.

But computational expressions are not just used for asynchronous operations, it can be used to create the `IEnumerable` equivalent: `seq`. People have used computational expressions to create domain specific language, [FsHttp](https://fsprojects.github.io/FsHttp/) is a builder for HTTP requests and there is one for [building a query](https://learn.microsoft.com/en-gb/dotnet/fsharp/language-reference/query-expressions).

You can write your own, but they are not always easy to make and could fill a whole book by themselves. So I won't be delving into them here. If you are curious, find more information in the [official docs](https://learn.microsoft.com/en-gb/dotnet/fsharp/language-reference/computation-expressions) and the [F# For Fun and Profit](https://fsharpforfunandprofit.com/posts/computation-expressions-intro/).

At the point that we return, we don't return one value, we return a tuple. This is a feature of Wolverine where I can first return the value of the operation (the `Results.Created`) and at the same time publish one (or more) messages. Wolverine then routes these through its message bus. This allows me to chain events together. I've created the drone, returned the result back to the caller and notified everybody in my system that I created a drone.

Fun little tidbit, in F# the default struct when using the notation `(first, second, third)` creates a `System.Tuple`. This is a bit different from C# where it creates a `ValueTuple`. This is because F# has had tuples for a very long time and the `ValueTuple` is a relatively new addition to C#. The `struct` keyword is used to create a `System.ValueTuple` instead of a `System.Tuple`. The `ValueTuple` is handled by Wolverine correctly whereas the regular `Tuple` would send all values as the return value for the HTTP request. That was a fun little headscratcher when I was first working with Wolverine in combination with F#.

## Handling a message
Handling the `DroneCreated` message is even easier than handling an http request. I just need to create a function that takes a `DroneCreated` message and any dependencies that I need. 

According to the Wolverine documentation, it can [discover handlers](https://wolverine.netlify.app/guide/handlers/) in a variety of ways. The way most C# developers will be familiar with is a type that ends with _Handler_ with a method that ends in _Handle_. In contrast to other frameworks or libraries that support this type of thing, it does not require any interfaces or base classes. It's just a convention.

```fsharp
module Drone.Api.Features.NotifySubscribersOfNewDrone
type Handler () =
    member this.Handle (message: DroneCreated) =
        printfn $"Does it work with Type? drone registered: {message}"
```

The `()` after the type name indicates that this type is not just a record, but a full fledged class. Never forget that F# is a hybrid programming language, it's functional first but has very good support for typing.

Now, this wouldn't be an F# blog post if there wasn't an even simpler way. Wolverine supports static methods in static classes as handlers. Since F# complies to static methods, this is a very easy way to create handlers.

```fsharp
module Drone.Api.Features.NotifySubscribersHandler

[<WolverineHandler>]
let notifyWithDelay (message: DroneCreated) (logger: ILogger) =
    task {
        do! Task.Delay(TimeSpan.FromSeconds 2)
        logger.LogInformation("Does it work with attributes? drone registered: {Message}", message)
    }

let Handle (message: DroneCreated) =
    printfn $"Does it work with correct naming? drone registered: {message}"
```

Notice that the module ends with the _Handler_ suffix and that we can have either a function called `Handle` or a function with the `[<WolverineHandler>]` attribute.

If I'd want, I could fine tune handler discovery during startup since Wolverine is quite flexible in this way. Wolverine also has a number of suffixes for classes and methods so be sure to check out their docs for all cases. I found these the most easy to start with.

## Registering a flight
At this point, you might be wondering why F# is better than C#. All I've shown so far is that it requires less brackets, it can create functions and records easier and it's backwards compatible with C#. Let me tell you how F# makes processing data a lot more readable and fault tolerant. That's where F# (and most functional languages) really shine.

So let me introduce the concept of a flight: a flight is a drone that will fly a certain route. A deceptively simple concept that hides quite the bit of complexity. To keep it all understandable, I'm going to ignore some aspects and say that all flights occur at the same time and at the same altitude. The resulting complexity will be enough to demonstrate my point.

```fsharp
type Coordinate = { Lat: int; Long: int }
type FlightPath =
    | TakeOff of Coordinate
    | Waypoint of Coordinate
    | Land of Coordinate

[<CLIMutable>]
type Flight = {
    Id: int
    DroneId: int
    Path: FlightPath list
}
```

The `Flight` type is straightforward enough, but... That's a strange notation for the `FlightPath`. That is because it is a discriminated union. A discriminated union is a type that can have a number of different values. Here they all have the same underlying type of `Coordinate`. That does not need to be the case as I'll demonstrate shortly.

Creating an endpoint that can handle this is quite simple.

```fsharp
[<Tags("Drone")>]
[<WolverinePost("drone/{droneId}/flight")>]
let registerFlight (droneId: int) (trajectory: FlightPath list) (db: DroneContext) =
    task {
        let! drone = db.Drones.FindAsync(droneId)
        let! existingFlights = db.Flights.ToListAsync()
        let existingFlights = existingFlights |> List.ofSeq
        let flightRegistered = validateFlight drone existingFlights trajectory
        let message = flightRegistered
        return 
            match flightRegistered with
            | FlightRejected reason -> struct (Results.NotFound(reason), message, SaveFlight(None))
            | FlightRegistered flight -> struct (Results.Ok(flight.Id), message, SaveFlight(Some flight))
    }
```

If the body of the request contains correct JSON, Wolverine will automatically deserialize it into the `FlightPath` list. The thing that you have to be aware of is that Wolverine, out of the box, uses the `System.Text.Json` library. It does not support discriminated unions out of the box. I had to configure Wolverine to use the `Newtonsoft.Json` library. This is not a problem as Wolverine is very flexible in this regard. It's just something to be aware of.

In the method body, I'm using a pattern that I'm growing ever more fond of: first load all the data that I will need and then process or validate it. After all processing is done, I can then save the data. Wolverine even helps with this in the form of side effects. This will make writing automated tests later a lot easier as I can focus on the processing and not on the loading and saving of data.

Before I'm going to focus on the validation part, I'm going to highlight the result creation. The output of the validation will be another discriminated union on which we can make decisions about the HTTP response and wether or not to save the flight to our database.

```fsharp
type FlightRegistered =
    | FlightRejected of string
    | FlightRegistered of Flight
```

Here we see that the options of a discriminated union can easily have different types associated with them. This makes them very good for communicating the available options. Combine that with the `match du with ...` syntax that will tell you when you have missed an option and you can write software that will be very robust as you cannot ignore an option.

Besides the self written discriminated unions, there are two very popular ones in F#: `Option` and `Result`.

```fsharp
type Option<'a> =
    | None
    | Some of 'a
Type Result<'T, 'TError> =
    | Ok of 'T
    | Error of 'TError
```

Generic types, which can be used in conjunction with discriminated unions, are noted with an apostrophe. This is to differentiate them from regular types. The `Option` denotes a possible value, as `Nullable<T>` does in C#. The `Result` denotes a value that can be an error or a value. They even have computational expressions that can be used to chain operations together. When `let!` is used and a `None` or `Error` is detected, the computation will stop and the `None` or `Error` will be returned.

When I add cases, the compiler gives me a nice warning that I have not handled all cases and where to find them. This makes finding missing features or bugs trivial. I recommend either paying very close attention to all the warnings that the F# compiler gives you or, which is even better, to treat them as errors.

```fsharp
type FlightRegistered =
    | FlightRejected of string
    | Collision of Coordinate
    | FlightRegistered of Flight

// compiler output
// 1>RegisterFlight.fs(92,19): Warning FS0025 : Incomplete pattern matches on this expression. For example, the value 'Collision (_)' may indicate a case not covered by the pattern(s).
// 2>ProcessDataAsync.fs(11,11): Warning FS0025 : Incomplete pattern matches on this expression. For example, the value 'Collision (_)' may indicate a case not covered by the pattern(s).
```

With a better understanding of discrimintated unions, lets take a look at the validation.

```fsharp
let validateFlight (drone: Drone) existingFlights newFlightPath =
    if obj.ReferenceEquals(drone, null) then
        FlightRejected($"Drone {drone.Id} not found")
    else
        existingFlights
        |> List.collect (fun flight -> detectCollisionAlongFlightPath newFlightPath flight.Path)
        // |> List.map _.Path
        // |> List.map2 detectCollisionAlongFlightPath newFlightPath
        // |> List.collect id // select many
        |> List.sort
        |> List.tryHead
        |> function
        | Some coordinate -> FlightRejected($"Collision detected at {coordinate.Lat}::{coordinate.Long}")
        | None -> FlightRegistered { Id = 0; DroneId = drone.Id; Path = newFlightPath }
```

The first part is how F# does a null check, this is a bit more work than in C# as F# generally does not allow nulls. In the case that we do have a drone, lets check whether the flight path is correct. 

The `List.collect` does two things at the same time, it applies a function to every item in the list and flattens the result such as the `SelectMany` Linq extension would do. The three commented-out lines below show an equivalent of the collect. The `List.map` function is the equivalent of the `Select` Linq extension used in combination with a shorthand notation for selecting a single property. The resulting type of the mapping is a `List<List<Flightpath>>`. The `List.map2` passes two lists to a function and returns a list of the results. The `List.collect` function is the equivalent of the `SelectMany` Linq extension and flattens the results into one list. That's a lot of work for one `List.collect` function.

The `List.sort` function is the equivalent of the `OrderBy` Linq extension and will give us the earliest collision. The `List.tryHead` function is the equivalent of the `FirstOrDefault` Linq extension in C#. It returns an `Option` type. The `function` keyword is used to match on resulting `Option`. It's a shorthand for the `match` keyword. Here, when a `Coordinate` is found, the flight is rejeted. When no `Coordinate` is found, the flight is valid.

With this information, we can check how the collision is detected along the flight path.

```fsharp
let detectCollisionAlongFlightPath (newFlightPath: FlightPath list) (existingFlightPath: FlightPath list) =
    let equal = List.min [newFlightPath.Length; existingFlightPath.Length] |> List.take
    List.map2 detectCollision (equal newFlightPath) (equal existingFlightPath)
    |> List.choose id // remove None
```

The first line creates a function `equal` that will take the number of elements from the shortest list. It does so by using _partial application_ of a function. In F#, we can create new functions by only applying the first paramters. Remember that order is important! For example:

```fsharp
let add a b = a + b
let plusTwo = add 2
```

The first line of our collision detection function uses this to create a function that will take the first elements of the supplied list. The `List.map2` function is the equivalent of the `Zip` Linq extension. It applies a function to two lists and returns a list of the results. So it takes the first element of the two lists and passes that combination to the `detectCollision` function.

The `detectCollision` returns a `Coordinate option` (which is an alternative way of writing `Option<Coordinate>`). The `List.choose` function is the equivalent of the `Where` Linq extension. With the specific case that it expects the function that is passed to it to return an `Option`.  It then removes all `None` values from the list. The `id` function is a simple function that just returns what is given to it. Since the list is of type `Coordinate option`, the `None` values are removed and we are left with a list of `Coordinate`. Basically, it removed the empty values from the list.

Now all that is left to do, is detect when a collision happens.
    
```fsharp
let collision first second =
    if first = second then
        Some first
    else
        None

let detectCollision newPath existingPath =
    match newPath, existingPath with
    | TakeOff coordinate, TakeOff existing -> collision coordinate existing
    | TakeOff coordinate, Land existing -> collision coordinate existing
    | Waypoint coordinate, Waypoint existing -> collision coordinate existing
    | Land coordinate, TakeOff existing -> collision coordinate existing
    | Land coordinate, Land existing -> collision coordinate existing
    | TakeOff _, Waypoint _ -> None
    | Waypoint _, TakeOff _ -> None
    | Waypoint _, Land _ -> None
    | Land _, Waypoint _ -> None
```

Now this is cool, I can put multiple values in a match statement and check all available combinations. This makes sure that no combination is forgotten, which in turn makes bugs really difficult to manifest. So all that is left to do is to check when a collision occurs. I think that right now, you can easily tell when one or the other happens.

The `collision` function is a simple function that checks if two coordinates are the same and returns an `Option` type. Since the `Coordinate` is a record, it has value equality and thus automatically checks whether all properties contain the same value.

The entire validation logic is about 35 lines long. 70 if you count all the `open` statements and the endpoint definition. This is a very small file for all the things that are going on and this is exactly why I love F#. I can express complex algorithms with relative ease.

## Side effects

Wolverine has this neat feature where I can easily define what a side effect is. Basically it's, you can guess it by now, a certain kind of message or event. In this case, the specific instruction to save a flight to the database. This can be any state altering action: updating a redis cache, writing to a file, making http calls. Wolverine lets me define a side effect through the marker interface `ISideEffect`.

```fsharp
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
```

The `SaveFlight` takes the information it needs to save the flight and it expects a method named `Execute` or `ExecuteAsync` to handle the work. The reason the method is not defined on the interface, is that you can't specify what is needed to be injected. For example, I can inject the `DbContext` into the `SaveFlight` side effect. The side effect can then be very small as it expects all the verification to be handled by whatever function created this effect.

## Testing

What would all this beautiful and concise code be worth, if it couldn't be easily tested? Luckily, testing is quite easy in F# as it can use xunit to do so. So all of your xunit knowledge can be reused here.

```fsharp
module ``Collisions Test``

open Drone.Api.Domain.Drone
open Drone.Api.Features
open Drone.Shared.Domain.Drone
open Messages
open Xunit

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
```

That's really nice, I can easly create existing flights, pass them along to the `RegisterFlight.validateFligth` function and check the result. The discriminated unions make checking the return value really simple.

Also notice the module and test names. They are surrounded with double backticks. F# allows you to write any identifier between double backticks and include special charactes or even sentences. I would not recommend this in production code as it makes referencing and calling functions a little harder as you'll need to write the names of the functions between double backticks as well. But for tests, it's really nice to be able to write a sentence as a test name. No snake or camel case needed.

## Processing messages in the background

The last thing that I want to show is processing messages in the background. Wolverine has support for several message brokers, such as RabbitMQ and Azure Service Bus. I'm going to show how to use RabbitMQ as it's a bit easier to set up and use. This is basic configuration for RabbitMQ and comes practically straight from the Wolverine documentation.

```fsharp
options
    .UseRabbitMq(Uri(config.GetConnectionString("RabbitMQ")))
    .EnableWolverineControlQueues()
    .AutoProvision()
    .UseConventionalRouting()
options.UseNewtonsoftForSerialization()
```

There is one thing to be aware of when it comes to discriminated unions. When you use them as messages, it all works very nice when you are using the memory bus and processing them locally. When you want to send them over the wire, you need to wrap them in a generic message type.

```fsharp
type DiscriminatedUnionMessage<'a> = { Payload: 'a }

let toMessage payload = { Payload = payload }
```

To understand why I need to do this, I need to explain that discriminated unions are basically an empty abstract type with several subtypes. When a message is sent over the wire, it will be deserialised into its specific case. Especially when one of the types is a base type such as `string`, it can be hard to match which handlers should be invoked. Wrapping it in this envelope (call it what you will), allows the router to correctly dispatch the message to the right handlers.

```fsharp
[<WolverineHandler>]
let flightRegistered (message: DiscriminatedUnionMessage<FlightRegistered>) =
    match message.Payload with
    | FlightRejected reason -> printfn $"Flight rejected because {reason}"
    | FlightRegistered flight -> printfn $"Flightpath added: {flight}"
```

It's a bit annoying, but it's a small price to pay for the power that discriminated unions give you.

## Architecture

I haven't talked about this explicitly, but Wolverine guides you into a vertical slice architecture. All features live in separate files or folders, segregating concepts and logic into their own little corners. This makes sharing code a bit harder, but still very doable. The upside is that when I want to change behavior of a certain feature, I only need to look in one place. I can also be very confident that it will not affect any other functionality.

This architecture is proving to be very useful as the sharing of code is, in my opinion, not as important as the ability to change code. In my experience, I've seen a lot of project take a lot of steps to make code sharable, yet it's very rare that code is actually shared. Changing code is much more common and thus should be easier. This is where Wolverine and F# guide you into the pit of success.

## Conclusion
Throughout the journey I've showed a few powerful features of functional languages and F# specifically. From easier type systems and piping, to the power of discriminated unions and computational expressions.

If you want to start small, F# has scripting support out of the box. So instead of writing a powershell script of small C# console to do some task. Open up VS Code, create a new file with the `.fsx` extension and start writing F#.

Don't be afraid to dip your toe into the functional world, it's not as scary as it seems.