namespace Shared

open System
open Microsoft.FSharp.Core

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: Todo -> Async<Todo> }



// Based on Enterprise Tic-Tac-Toe Talk @ 14.55 (https://vimeo.com/131196782)
// Make UI life easier by explicitly returning changes with each move
// ---
// Use System collections for native js types
// See: https://fable.io/docs/dotnet/compatibility.html
type Dictionary<'K, 'V> = System.Collections.Generic.Dictionary<'K, 'V>
module DomainDto =

    type TileDto =
        | Land = 0
        | Water = 1
        | Mountain = 2

    type BoardDto = ResizeArray<ResizeArray<TileDto>>

    type CharacterClassDto =
        | Axe = 0
        | Sword = 1
        | Lance = 2
        | Bow = 3
        | Support = 4

    type MoveTypeDto =
        | Foot = 0
        | Mount = 1
        | Fly = 2

    type PositionDto = { row: int; col: int }

    type PlayerDto =
        | Player1 = 0
        | Player2 = 1

    type CharacterIdDto = string

    type CharacterDto =
        { id: CharacterIdDto
          name: string
          classification: CharacterClassDto
          position: Option<PositionDto>
          player: Option<PlayerDto>
          properties: Dictionary<string, Object> }


    type PlaceCharacterDto =
        { player: PlayerDto
          character: CharacterDto
          pos: PositionDto }

    type SelectableActionDto = string
    type IMessage =
        | SelectCharacterDto of CharacterIdDto
        | DeselectCharacterDto
        | MoveCharacterDto of PositionDto
        | SelectActionDto of SelectableActionDto
        | DeselectActionDto
        | PerformActionDto of CharacterIdDto

    type StartResult =
        { id: string
          board: BoardDto
          characters: ResizeArray<CharacterDto> }

    type PlayerOverseeResult = {
        player: PlayerDto
        selectableCharacters: ResizeArray<CharacterIdDto>
    }

    type PlayerMoveSelectionResult =
        { character: string
          availableMoves: ResizeArray<PositionDto> }

    type CharacterUpdateResult = {
        character: CharacterDto
    }

    type PlayerActionSelectionResult = {
        availableActions: ResizeArray<SelectableActionDto>
    }

    type ActionPreviewDto = {
        name: string
        results: Dictionary<CharacterIdDto, CharacterDto>
    }

    type PlayerActionResult = {
        selectableCharacters: ResizeArray<CharacterIdDto>
        preview: option<ActionPreviewDto>
    }

    type CharacterDefeatResult = {
        character: CharacterIdDto
    }

    type PlayerWinResult = {
        player: PlayerDto
    }

    type ErrorMsg = string

    type IResult =
        | StartResult of StartResult
        | PlayerOverseeResult of PlayerOverseeResult
        | PlayerMoveSelectionResult of PlayerMoveSelectionResult
        | CharacterUpdateResult of CharacterUpdateResult
        | PlayerActionSelectionResult of PlayerActionSelectionResult
        | PlayerActionResult of PlayerActionResult
        | CharacterDefeatResult of CharacterDefeatResult
        | PlayerWinResult of PlayerWinResult
        | UnsupportedResult of ErrorMsg



open DomainDto

type GameInfo = {
    id: string
    player: PlayerDto
}

type IGameApi =
    { start: unit -> Async<GameInfo>
      poll: string -> PlayerDto -> Async<Option<IResult>>
      update: string -> PlayerDto -> IMessage -> Async<unit> }
