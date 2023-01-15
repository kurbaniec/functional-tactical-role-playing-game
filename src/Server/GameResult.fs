module GameResult

open Microsoft.FSharp.Core
open Shared.DomainDto
open FSharpx.Collections
open Utils
open Position

let recipient (result: GameResult) (state: GameState) : Recipient =
    let turnOf = state |> GameState.turnOf

    match result with
    | Start _ -> AllRecipients
    | PlayerOversee -> AllRecipients
    | PlayerMoveSelection -> PlayerRecipient turnOf
    | CharacterUpdate _ -> AllRecipients
    | PlayerActionSelection -> PlayerRecipient turnOf
    | PlayerAction -> PlayerRecipient turnOf
    | CharacterDefeat _ -> AllRecipients
    | PlayerWin -> AllRecipients

let intoTileDto (tile: Tile) : TileDto =
    match tile with
    | Land _ -> TileDto.Land
    | Water _ -> TileDto.Water

let intoPositionDto (pos: Position) : PositionDto =
    let row, col = pos

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

let intoMoveTypeDto (moveType: MovementType) : MoveTypeDto =
    match moveType with
    | Foot -> MoveTypeDto.Foot
    | Mount -> MoveTypeDto.Mount
    | Fly -> MoveTypeDto.Fly

let intoPlayerDto (player: Player) : PlayerDto =
    match player with
    | Player1 -> PlayerDto.Player1
    | Player2 -> PlayerDto.Player2

type PropertyDictionary = System.Collections.Generic.Dictionary<string, System.Object>

let intoCharacterDto (c: Character) (b: Option<Board>) (p: Option<Player>) : CharacterDto =
    let id = c.id.ToString()
    let name = c.name
    let cls = intoClsDto c.stats.cls

    let properties =
        PropertyDictionary()
        |> Dictionary.add "atk" (c |> Character.atk :> obj)
        |> Dictionary.add "hp" (c |> Character.hp :> obj)
        |> Dictionary.add "maxHp" (c |> Character.maxHp :> obj)
        |> Dictionary.add "def" (c |> Character.def :> obj)
        |> Dictionary.add "mv" (c.movement.distance |> Movement.Distance.value :> obj)
        |> Dictionary.add "mvType" (c.movement.kind |> intoMoveTypeDto :> obj)
        |> fun dict ->
            let heal = c |> Character.heal

            if heal > 0 then
                dict |> Dictionary.add "heal" heal
            else
                dict

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

let intoStartDto (gid: GameId) (game: GameState) =
    let boardDto = intoBoardDto game.board

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


let intoPlayerOverseeDto (game: GameState) =
    let selectableCharacters =
        game
        |> GameState.awaitingTurns
        |> Map.values
        |> Seq.map (fun c -> c.id.ToString())
        |> ResizeArray

    PlayerOverseeResult
        { player = intoPlayerDto <| game.turnOf
          selectableCharacters = selectableCharacters }

let intoPlayerMoveSelectionDto (state: GameState) (phase: PlayerMove) =
    PlayerMoveSelectionResult
        { character = phase.character.id.ToString()
          availableMoves = phase.availableMoves |> List.map intoPositionDto |> ResizeArray }



let intoCharacterUpdateDto (cid: CharacterId) (game: GameState) =
    let character =
        Map.join game.player1Characters game.player2Characters
        |> Map.find cid
        |> fun c -> intoCharacterDto c (Some game.board) None

    CharacterUpdateResult { character = character }

let intoPlayerActionSelectionDto (state: GameState) (phase: PlayerActionSelect) =
    PlayerActionSelectionResult
        { availableActions = phase.availableActions |> List.map (fun a -> a.action.name) |> ResizeArray }

let intoPlayerActionDto (state: GameState) (phase: PlayerAction) =
    let selectableAction = phase.action

    let selectableCharacters =
        selectableAction.selectableCharacters
        |> List.map (fun cid -> cid.ToString())
        |> ResizeArray

    let preview =
        selectableAction.preview
        |> Option.map (fun characters -> characters |> Map.toList)
        |> Option.map (fun characters ->
            characters
            |> List.map (fun (cid, c) -> (cid.ToString(), intoCharacterDto c None None)))
        |> Option.map (fun charactersDto -> charactersDto |> Dictionary.ofList)
        |> Option.map (fun charactersDto ->
            { name = selectableAction.action.name
              results = charactersDto })

    PlayerActionResult
        { selectableCharacters = selectableCharacters
          preview = preview }

let intoCharacterDefeatDto (cid: CharacterId) =
    CharacterDefeatResult { character = cid.ToString() }

let intoPlayerWinDto (game: GameState) =
    PlayerWinResult { player = intoPlayerDto <| game.turnOf }


let intoDto (result: GameResult) (state: GameState) =
    let phase = state |> GameState.phase

    match phase, result with
    | _, Start guid -> intoStartDto guid state
    | _, PlayerOversee -> intoPlayerOverseeDto state
    | PlayerMovePhase phase, PlayerMoveSelection -> intoPlayerMoveSelectionDto state phase
    | PlayerActionSelectPhase phase, PlayerActionSelection -> intoPlayerActionSelectionDto state phase
    | PlayerActionPhase phase, PlayerAction -> intoPlayerActionDto state phase
    | PlayerWinPhase, PlayerWin -> intoPlayerWinDto state
    | _, CharacterUpdate cid -> intoCharacterUpdateDto cid state
    | _, CharacterDefeat cid -> intoCharacterDefeatDto cid
    | _ -> failwith "intoDto" // TODO not supported
