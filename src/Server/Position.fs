namespace Position

module Row =
     let value (Row r) = r

module Col =
    let value (Col c) = c

module Position =
    let value (c: Position) =
        let (row, col) = c
        let row = row |> Row.value
        let col = col |> Col.value
        (row, col)

    let row (pos: Position) = fst pos
    let col (pos: Position) = snd pos

    let add (r: Row) (c: Col) (cell: Position) : Position =
        let row = cell |> row |> Row.value |> (+) <| Row.value r
        let col = cell |> col |> Col.value |> (+) <| Col.value c
        (Row row, Col col)
