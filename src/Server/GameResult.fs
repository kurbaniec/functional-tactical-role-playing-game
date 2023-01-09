module GameResult


open GameState
open Microsoft.FSharp.Core
open Shared.DomainDto
open FSharpx.Collections
open Utils

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
    let cls = intoClsDto c.stats.cls
    let properties = System.Collections.Generic.Dictionary<string, System.Object>()
    // TODO properties
    properties.Add("Attack", c |> Character.atk)
    properties.Add("HP", c |> Character.hp)
    properties.Add("DEF", c |> Character.def)
    c |> Character.heal |> fun heal -> if heal > 0 then properties.Add("HEAL", heal)

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

    // let characters =
    //     Map.join game.player1Characters game.player2Characters
    //     |> Map.values
    //     |> ResizeArray
    //     |> ResizeArray.map (fun c -> intoCharacterDto c <| Some game.board <| Some Player1)
    // TODO refactor this
    let characters1 =
        game.player1Characters
        |> Map.values
        |> ResizeArray
        |> ResizeArray.map (fun c -> intoCharacterDto c <| Some game.board <| Some Player1)

    let characters2 =
        game.player2Characters
        |> Map.values
        |> ResizeArray
        |> ResizeArray.map (fun c -> intoCharacterDto c <| Some game.board <| Some Player2)
    let characters = characters1 |> ResizeArray.append characters2

    StartResult
        { id = gid.ToString()
          board = boardDto
          characters = characters }


let intoPlayerOverseeDto (game: GameDetails) =
    PlayerOverseeResult { player = intoPlayerDto <| game.turnOf }

let intoPlayerMoveSelectionDto
    (player: Player)
    (cid: CharacterId)
    (availableMoves: list<CellPosition>)
    (game: GameDetails)
    =
    PlayerMoveSelectionResult
        { character = cid.ToString()
          availableMoves = availableMoves |> List.map intoPositionDto |> ResizeArray }



let intoCharacterUpdateDto (cid: CharacterId) (game: GameDetails) =
    let character =
        Map.join game.player1Characters game.player2Characters
        |> Map.find cid
        |> fun c -> intoCharacterDto c (Some game.board) None

    CharacterUpdateResult { character = character }

let intoPlayerActionSelectionDto (p: Player) (actions: ApplicableActions) =
    PlayerActionSelectionResult { availableActions = actions |> List.map (fun a -> a.action.name) |> ResizeArray }

let intoPlayerActionDto (player: Player) (cids: list<CharacterId>) =
    PlayerActionResult { selectableCharacters = cids |> List.map (fun cid -> cid.ToString()) |> ResizeArray }

let intoCharacterDefeatDto (cid: CharacterId) =
    CharacterDefeatResult { character = cid.ToString() }

let intoPlayerWinDto (game: GameDetails) =
    PlayerWinResult { player = intoPlayerDto <| game.turnOf }


let intoDto (gameResult: GameResult) (game: GameDetails) =
    let res: IResult =
        match gameResult with
        | Start guid -> intoStartDto guid game
        | PlayerOversee -> intoPlayerOverseeDto game
        | PlayerMoveSelection (p, c, m) -> intoPlayerMoveSelectionDto p c m game
        | CharacterUpdate cid -> intoCharacterUpdateDto cid game
        | PlayerActionSelection (p, a) -> intoPlayerActionSelectionDto p a
        | PlayerAction (p, cids) -> intoPlayerActionDto p cids
        | CharacterDefeat cid -> intoCharacterDefeatDto cid
        | PlayerWin -> intoPlayerWinDto game
        | _ -> failwith "intoDto"

    res
