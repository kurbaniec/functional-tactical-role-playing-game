module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Shared.DomainDto

module Storage2 =
    let todos = ResizeArray()

    let addTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

    do
        addTodo (Todo.create "Create new SAFE project") |> ignore

        addTodo (Todo.create "Write your app") |> ignore
        addTodo (Todo.create "Ship it !!!") |> ignore

let todosApi =
    { getTodos = fun () -> async { return Storage2.todos |> List.ofSeq }
      addTodo =
        fun todo ->
            async {
                return
                    match Storage2.addTodo todo with
                    | Ok () -> todo
                    | Error e -> failwith e
            } }

module Storage =
    let games =
        System.Collections.Concurrent.ConcurrentDictionary<System.Guid, System.Object>()

let gameApi =
    { start =
        fun () ->
            async {
                let words = System.Collections.Generic.Dictionary<string, System.Object>()
                words.Add("1", "book")
                words.Add("2", 2)

                let c: CharacterDto =
                    { id = ""
                      name = ""
                      classification = CharacterClassDto.Axe
                      properties = words

                    }

                let char: PlaceCharacterDto =
                    { player = PlayerDto.Player1
                      character = c
                      pos = { row = 1; col = 2 }

                    }

                let test: StartResult =
                    { id = "test"
                      board =
                        System.Collections.Generic.List(
                            [ System.Collections.Generic.List([ TileDto.Land; TileDto.Water ]) ]
                        )
                      characters = System.Collections.Generic.List([ char ]) }

                return test
            } }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue gameApi
    |> Remoting.buildHttpHandler

let app =
    application {
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    run app
    0
