module Board

let create (row: Row) (col: Col): Board =
    let columns =
        [ for i in 0 .. (Col.value col)-1 -> ( Col i, Tile.Land None) ]
        |> Map.ofList
    let board =
        [ for i in 0 .. (Row.value row)-1 -> ( Row i, columns)]
        |> Map.ofList
    board

let placeCharacter (pos: CellPosition) (c: Character) (board: Board): Board =
    let (row, col) = pos
    let colMap = board[row]
    let existingTile = colMap[col]
    // Check if occupied
    // TODO fail when occupied
    let newTile =
        match existingTile with
        | Land _ -> Land <| Some c.id
        | Water _ -> Land <| Some c.id
    let newColMap = colMap.Add(col, newTile)
    let newBoard = board.Add(row, newColMap)
    newBoard

let moveCharacter (f: CellPosition) (t: CellPosition) (board: Board): Board =
    let (row, col) = f
    // TODO
    failwith "not implemented"



    board


