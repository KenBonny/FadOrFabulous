module Drone.Api.WolverineOperationFilter

open Swashbuckle.AspNetCore.SwaggerGen
open Wolverine.Http

type WolverineOperationFilter() =
    interface IOperationFilter with
        member this.Apply(operation, context) =
            match context.ApiDescription.ActionDescriptor with
            | :? WolverineActionDescriptor as wolverine ->
                operation.OperationId <- wolverine.Chain.OperationId
            | _ -> ()