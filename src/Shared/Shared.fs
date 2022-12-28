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

module Dto =
    type Input =
        | Left
        | Right
        | Up
        | Down
        | Action
        | Back
        | Start

    
