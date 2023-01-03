module Board

open System
open System.Collections.Generic

let create (row: Row) (col: Col) : Board =
    let columns =
        [ for i in 0 .. (Col.value col) -> (Col i, Tile.Land None) ]
        |> List.mapi (fun i x -> if i = 1 || i = 5 then (Col i, Tile.Water None) else x)
        |> Map.ofList

    let board = [ for i in 0 .. (Row.value row) - 1 -> (Row i, columns) ] |> Map.ofList
    board

let placeCharacter (pos: CellPosition) (c: CharacterId) (board: Board) : Board =
    let (row, col) = pos
    let colMap = board[row]
    let existingTile = colMap[col]
    // Check if occupied
    // TODO fail when occupied
    let newTile =
        match existingTile with
        | Land _ -> Land <| Some c
        | Water _ -> Land <| Some c

    let newColMap = colMap.Add(col, newTile)
    let newBoard = board.Add(row, newColMap)
    newBoard

let findCharacter (c: CharacterId) (board: Board) : CellPosition =
    board
    |> Map.pick (fun row colMap ->
        colMap
        |> Map.tryPick (fun col tile ->
            match (Tile.characterId tile) with
            | Some cid -> if cid = c then Some(row, col) else None
            | None -> None))

let moveCharacter (f: CellPosition) (t: CellPosition) (board: Board) : Board =
    let (row, col) = f
    // TODO
    failwith "not implemented"
