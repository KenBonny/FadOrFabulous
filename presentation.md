# F#: Fad or Fab?

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
- Getting drone data
  - functions
  - lambda functions
  - simplified types
- Creating a new drone
  - computational expressions: task, others such as http and sql
  - let! and do!
- Handling a message
- registering a flight
  - discriminated unions
  - pipe operator
  - pattern matching
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
Welcome to this talk on F# and Wolverine. I'm going to be talking about how I used F# and Wolverine to build a drone delivery system. It's a very simple example system so I'm hoping that it will be easy to follow along. I hope that I can convey that F# is not as difficult as it may seem and that it's a very viable option for building systems.

What I'm not going to do is go into every minute detail of F#. There are a lot of introduction to F# articles out there, so I'm going to focus more on the more useful patterns and practices that make F# such a joy to use. The code I write here should be easy to understand, even if you've never seen F# before.

## The technology used
Well, first off there is F#. It's a functional dotnet language. As you follow along, you might see where C# gets its inspiration from. F# is a very powerful language and it can also be very simple. It's a very good language for writing code that is easy to understand and maintain. Of course, such as with any language, you can write difficult to understand code in F# too.

I'm pairing it with Wolverine. Wolverine is a framework that makes it easy to process messages. It can be seen as an alternative to NServiceBus or MassTransit. It's a very simple framework that makes it easy to process messages. It makes use of static functions a lot and F#'s functions compile to static functions, so the two pair quite well. I've also chosen it to show that interop between F# and C# is quite easy. Things that are written in one, can easily be used in the other.

Lastly, the major technology used is Entity Framework. This is mainly done to show that F# and EF work well together. Support in C# is better, but F# is still good.

## Getting drone data
The most simple thing that I can start with is retrieving drone data. This will take the form of an http endpoint that can be called. I'm going to use a simple function to do this. With the `let` keyword, I can define both functions as well as values, for more information check the [official docs](https://fsharp.org/docs/) and the [cheat sheet](https://github.com/fsprojects/fsharp-cheatsheet/blob/master/docs/fsharp-cheatsheet.md) or [F# for fun and profit](https://fsharpforfunandprofit.com/posts/fsharp-in-60-seconds/).

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

A last point to note is the strange notation of the attributes `[<Tags>]` and `[<WolverineGet>]`. These are attributes that Wolverine uses to know how to route the request. The `Tags` attribute is used to group endpoints together. The reason that it's different from C# is that F# has collection initialisers for a very long time. `[1; 2; 3]` creates an F# list of `int`s and `[| 1; 2; 3 |]` creates an array. I'm again referring to the docs, the cheat sheet and F# for fun and profit for more information on colletions in F#. Let me just add that I find a bit confusing that almost every programming language uses square brackets for arrays and F# uses them for lists. This is because the F# list is much more used and thus got the easier notation. That does not make it any less confusing.

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