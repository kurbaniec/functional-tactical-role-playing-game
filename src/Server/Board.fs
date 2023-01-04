﻿module Board

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

let private updateTile (tile: Tile) (pos: CellPosition) (b: Board): Board =
    let (row, col) = pos
    b
    |> Map.change row (fun colMap ->
        match colMap with
        | None -> None
        | Some colMap ->
            colMap
            |> Map.change col (fun t ->
                match t with
                | None -> None
                | Some _ -> Some tile)
            |> Some)


let moveCharacter (cid: CharacterId) (target: CellPosition) (board: Board) : Board =
    let from = findCharacter cid board
    let fromTile = findTile from board |> Tile.leave
    let targetTile = findTile target board |> Tile.occupy cid
    board
    |> updateTile fromTile from
    |> updateTile targetTile target

let findNeighbors (pos: CellPosition) (b: Board) : list<CellPosition * Tile> =
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

type Frontier = list<CellPosition>
type FoundTile = { tile: Tile; distance: int }
type FoundTiles = Map<CellPosition, FoundTile>
type Neighbors = list<CellPosition * Tile>

// Adapted from on https://www.redblobgames.com/pathfinding/tower-defense/
let rec inspectNeighbors
    (currentDistance: int)
    (frontier: Frontier)
    (foundTiles: FoundTiles)
    (neighbors: Neighbors)
    : (Frontier * FoundTiles * int) =
    match neighbors with
    | [] -> (frontier, foundTiles, currentDistance)
    | neighbor :: neighbors ->
        let (nPos, nTile) = neighbor

        if not <| Map.containsKey nPos foundTiles then
            let frontier = nPos :: frontier

            let foundTiles =
                foundTiles
                |> Map.add
                    nPos
                    { tile = nTile
                      distance = currentDistance }

            inspectNeighbors currentDistance frontier foundTiles neighbors
        else
            inspectNeighbors currentDistance frontier foundTiles neighbors


let rec doPathfinding
    (frontier: Frontier)
    (foundTiles: FoundTiles)
    (maxDistance: int)
    (predicate: Tile -> bool)
    (b: Board)
    : FoundTiles =
    match frontier with
    | [] -> foundTiles
    | current :: frontier ->
        let currentDistance = Map.find current foundTiles |> fun ft -> ft.distance + 1

        if currentDistance > maxDistance then
            doPathfinding frontier foundTiles maxDistance predicate b
        else
            let neighbors = findNeighbors current b |> List.filter (fun n -> predicate (snd n))

            let (frontier, foundTiles, newDistance) =
                inspectNeighbors currentDistance frontier foundTiles neighbors

            doPathfinding frontier foundTiles maxDistance predicate b

let pathfinding
    (start: CellPosition)
    (maxDistance: int)
    (predicate: Tile -> bool)
    (extract: (FoundTiles) -> 'U)
    (b: Board)
    =
    let frontier = [ start ]
    let startTile = findTile start b
    let foundTiles = Map [ (start, { tile = startTile; distance = 0 }) ]

    doPathfinding frontier foundTiles maxDistance predicate b |> extract

let availablePlayerMoves (d: Distance) (c: CharacterId) (b: Board) : list<CellPosition> =
    let startPos = findCharacter c b
    let d = Distance.value d
    // TODO
    let predicate (tile: Tile) : bool = true
    // See: https://devonburriss.me/converting-fsharp-csharp/
    // let extract (found: FoundTiles): list<CellPosition> =
    //     found
    //     |> Map.values
    //     |> fun v -> v :> seq<_>
    //     |> Seq.map fst
    //     |> Seq.toList
    let extract (found: FoundTiles) : list<CellPosition> =
        found |> Map.keys :> seq<_> |> Seq.toList

    pathfinding startPos d predicate extract b
