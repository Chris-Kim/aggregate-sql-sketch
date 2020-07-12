module Sketch.Aggregate

open Sketch.Events
open Sketch.Persistence

let zero id = { Id = id; Items = []; Status = CartStatus.Pending }

let persist event conn =
  match event with
  | CartCreated cartId -> 
    match Cart.create cartId conn with
    | Ok _ -> Ok (sprintf "Cart Created: %A" cartId)
    | Error e -> Error (sprintf "Could create cart: %A" e)

  | ItemAddedToCart item -> 
    match Cart.addItemToCart item.CartId item.Data.Sku conn with
    | Ok _ -> Ok (sprintf "Added %A to cart %A" item.Data.Sku item.CartId)
    | Error e -> Error (sprintf "Item Add Error: %A" e)

  | ItemRemovedFromCart item -> 
    match Cart.removeItemFromCart item.CartId item.Data.Sku conn with
    | Ok _ -> Ok (sprintf "Removed %A from cart: %A" item.Data.Sku item.CartId)
    | Error e -> Error (sprintf "Item Remove Error: %A" e)
    
  | CheckedOut cartId -> 
    match Cart.checkout cartId conn with
    | Ok _ -> Ok (sprintf "Checked out %A" cartId)
    | Error e -> Error (sprintf "Checkout Error: %A" e)

// Read initial DB state -> Hydrate
let hydrate (id: int) conn = 
  Cart.loadCart id conn
  |> function
  | Ok c -> c
  | Error e -> 
    printfn "Hydration error: %A, starting from zero" e
    zero id