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


// Use System collections for native js types
// See: https://fable.io/docs/dotnet/compatibility.html
module DomainDto =

    type TileDto =
        | Land = 0
        | Water = 1

    type BoardDto = System.Collections.Generic.List<System.Collections.Generic.List<TileDto>>

    type CharacterClassDto =
        | Axe = 0
        | Sword = 1
        | Lance = 2
        | Bow = 3
        | Support = 4

    type PositionDto = { row: int; col: int }

    type PlayerDto =
        | Player1 = 0
        | Player2 = 0

    type CharacterDto =
        { id: string
          name: string
          classification: CharacterClassDto
          properties: System.Collections.Generic.Dictionary<string, Object>
          position: Option<PositionDto>
          }


    type PlaceCharacterDto =
        { player: PlayerDto
          character: CharacterDto
          pos: PositionDto }

    type IMessage =
        interface
        end

    type IResult =
        interface
        end

    type StartMessage() =
        interface IMessage

    type StartResult =
        { id: string
          board: BoardDto
          characters: ResizeArray<CharacterDto>
          }

        interface IResult

    type PlayerOverseeResult =
        { player: PlayerDto }

        interface IResult



    // type Input =
    //     | Left
    //     | Right
    //     | Up
    //     | Down
    //     | Action
    //     | Back
    //     | Start



open DomainDto

type IGameApi =
    { start: unit -> Async<StartResult>
    }