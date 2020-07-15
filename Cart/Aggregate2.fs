namespace Sketch.Aggregate2.Cart

open Sketch.Events.Cart
open aggregate_sql_sketch.Cart

type DomainError =
    | CartIsNotFound
    | CartAlreadyExists
    | FailedToAddCart
    | NotGood

module Aggregate =
    let zero id =
        { Id = id
          Items = []
          Status = CartStatus.Initial }

    let exec
        (state: CustomerCart)
        (command: CartCommand)
        : Result<CartEvent list, DomainError> =

        match command with
        | CreateCart a -> Ok [CartCreated { Id = a.Id }]

    let apply
        (state: CustomerCart)
        (event: CartEvent)
        : CustomerCart =

        match event with
        | CartCreated a -> { state with Id = a.Id }
