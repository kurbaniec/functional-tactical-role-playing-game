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

let validCharacterMoves (c: CharacterId) (board: Board) : list<CellPosition> =

    []

let containsPosition (pos: CellPosition) (b: Board) : bool =
    let (row, col) = pos

    b
    |> Map.tryFind row
    |> Option.map (Map.containsKey col)
    |> Option.defaultValue false

let tryFindTile (pos: CellPosition) (b: Board) : Option<Tile> =
    let (row, col) = pos
    b |> Map.tryFind row |> Option.map (Map.tryFind col) |> Option.flatten

let findTile (pos: CellPosition) (b: Board) : Tile =
    let (row, col) = pos
    b |> Map.find row |> Map.find col


let neighbors (pos: CellPosition) (b: Board) : list<CellPosition * Tile> =
    // let row, col = pos

    let newPos =
        [ (Row 0, Col -1, pos) // Up
          (Row 0, Col 1, pos) // Down
          (Row -1, Col 0, pos) // Left
          (Row 1, Col 0, pos) ] // Right

    newPos
    |> List.map (fun pos -> pos |||> CellPosition.add)
    |> List.filter (fun pos -> containsPosition pos b)
    |> List.map (fun pos -> (pos, (findTile pos b)))


let pathfinding
    (start: CellPosition)
    (distance: int)
    (predicate: Tile -> bool)
    (extract: (CellPosition * Tile) -> 'U)
    (b: Board)
    =


    ()
