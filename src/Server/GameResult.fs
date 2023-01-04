﻿module GameResult


open Microsoft.FSharp.Core
open Shared.DomainDto
open FSharpx.Collections

let intoTileDto (tile: Tile) : TileDto =
    match tile with
    | Land _ -> TileDto.Land
    | Water _ -> TileDto.Water

let intoPositionDto (pos: CellPosition) : PositionDto =
    let (row, col) = pos

    { row = Row.value row
      col = Col.value col }

let intoBoardDto (board: Board) : BoardDto =
    // TODO: rewrite more functional?
    let boardDto = ResizeArray<ResizeArray<TileDto>>()

    for r in 0 .. board.Count - 1 do
        let colMap = board[Row r]
        boardDto.Add(ResizeArray())

        for c in 0 .. colMap.Count - 1 do
            let tile = colMap[Col c]
            let tileDto = intoTileDto tile
            boardDto[ r ].Add(tileDto)

    boardDto

let intoClsDto (cls: CharacterClass) : CharacterClassDto =
    match cls with
    | Axe -> CharacterClassDto.Axe
    | Sword -> CharacterClassDto.Sword
    | Lance -> CharacterClassDto.Lance
    | Bow -> CharacterClassDto.Bow
    | Support -> CharacterClassDto.Support

let intoPlayerDto (player: Player) : PlayerDto =
    match player with
    | Player1 -> PlayerDto.Player1
    | Player2 -> PlayerDto.Player2

let intoCharacterDto (c: Character) (b: Option<Board>) (p: Option<Player>) : CharacterDto =
    let id = c.id.ToString()
    let name = c.name
    let cls = intoClsDto c.classification
    let properties = System.Collections.Generic.Dictionary<string, System.Object>()
    // TODO properties
    properties.Add("attack", 2)

    let position =
        match b with
        | Some b -> Board.findCharacter c.id b |> intoPositionDto |> Some
        | None -> None

    let player =
        match p with
        | Some p -> p |> intoPlayerDto |> Some
        | None -> None

    { id = id
      name = name
      classification = cls
      properties = properties
      position = position
      player = player }

let intoStartDto (gid: GameId) (game: GameDetails) =
    let boardDto = intoBoardDto game.board

    let characters =
        game.player1Characters.Values
        |> ResizeArray
        |> ResizeArray.map (fun c -> intoCharacterDto c <| Some game.board <| Some Player1)

    StartResult
        { id = gid.ToString()
          board = boardDto
          characters = characters }


let intoPlayerOverseeDto (player: Player) (game: GameDetails) =
    PlayerOverseeResult { player = intoPlayerDto player }

let intoPlayerMoveSelectionDto
    (player: Player)
    (cid: CharacterId)
    (availableMoves: list<CellPosition>)
    (game: GameDetails)
    =
    PlayerMoveSelectionResult
        { character = cid.ToString()
          availableMoves = availableMoves |> List.map intoPositionDto |> ResizeArray }


let intoDto (gameResult: GameResult) (game: GameDetails) =
    let res: IResult =
        match gameResult with
        | Start guid -> intoStartDto guid game
        | PlayerOversee player -> intoPlayerOverseeDto player game
        | PlayerMoveSelection (p, c, m) -> intoPlayerMoveSelectionDto p c m game
        | _ -> failwith "intoDto"

    res
