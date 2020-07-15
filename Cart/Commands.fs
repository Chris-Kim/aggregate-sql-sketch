namespace aggregate_sql_sketch.Cart

type CreateCart = { Id: int }

type CartCommand =
    | CreateCart of CreateCart
