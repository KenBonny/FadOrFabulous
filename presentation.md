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

That is why I'm pairing it with Wolverine. Wovlerine is a framework that makes it easy to process messages. It can be seen as an alternative to NServiceBus or MassTransit. It's a very simple framework that makes it easy to process messages. It makes use of static functions a lot and F#'s functions are static functions, so the two pair quite well.

Lastly, the major technology used is Entity Framework. This is mainly done to show that F# and EF work well together. Support in C# is better, but F# is catching up.