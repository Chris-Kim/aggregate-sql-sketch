module aggregate_sql_sketch.Domain.Handlers.Cart

open System.Data
open FSharpx.Control
open Dapper.FSharp // Looks very easy to use. https://github.com/Dzoukr/Dapper.FSharp
open Dapper.FSharp.MSSQL
open Sketch.Aggregate2.Cart
open Microsoft.FSharp.Core

open Sketch.Events.Cart
open aggregate_sql_sketch.Cart

let loadCart (conn: IDbConnection) txn cartId =
    let query = select {
        table "Cart"
        where (eq "Id" cartId)
    }
    conn.SelectAsync<CustomerCart>(query, txn)
    |> Async.AwaitTask
    |> Async.map Seq.tryHead

let saveCart (conn: IDbConnection) txn (cart: CustomerCart) =
    let query = insert {
        table "Cart"
        value cart
    }
    conn.InsertAsync(query, txn)
    |> Async.AwaitTask

let applyEvents state events = events |> List.fold (fun s v -> Aggregate.apply s v) state

let CreateCartHandler (conn: IDbConnection) (payload: CreateCart) : Async<Result<CartEvent list, DomainError>> = async {
    let cartId = payload.Id
    let txn = conn.BeginTransaction()
    let! cart = loadCart conn txn cartId

    match cart with
    | Some { Status = CartStatus.Initial } -> return Error CartAlreadyExists
    | None -> return Error CartIsNotFound
    | Some cart ->
        let execResults = Aggregate.exec cart (CreateCart payload)

        match execResults with
        | Ok events ->
            let! result = events |> applyEvents cart |> saveCart conn txn

            txn.Commit()
            if result > 0 then
                //TODO: Persist these events for other handlers
                return Ok events
            else return Error FailedToAddCart
        | Error e -> return Error e
}
