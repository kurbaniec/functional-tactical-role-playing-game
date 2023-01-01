namespace Shared

open System

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

module DomainDto =

    type TileDto =
        | Land = 0
        | Water = 1

    type BoardDto = List<List<TileDto>>

    type CharacterClassDto =
        | Axe = 0
        | Sword = 1
        | Lance = 2
        | Bow = 3
        | Heal = 4

    type PositionDto = { row: int; col: int }

    type PlayerDto =
        | Player1 = 0
        | Player2 = 0

    type CharacterDto =
        { id: string
          name: string
          classification: CharacterClassDto
          properties: Map<string, Object> }

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

    type Start() =
        interface IMessage

    type StartResult =
        { id: string
          board: BoardDto
          characters: List<PlaceCharacterDto> }

        interface IResult

    


    type Input =
        | Left
        | Right
        | Up
        | Down
        | Action
        | Back
        | Start
