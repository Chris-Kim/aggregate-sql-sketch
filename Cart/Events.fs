namespace aggregate_sql_sketch.Cart

type CartCreated = { Id: int }

type CartEvent =
    | CartCreated of CartCreated
