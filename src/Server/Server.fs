module Server

open System.Collections.Generic
open Domain.GameResult
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
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

type ConcurrentDictionary<'K, 'V> = System.Collections.Concurrent.ConcurrentDictionary<'K, 'V>
type Dictionary<'K, 'V> = System.Collections.Generic.Dictionary<'K, 'V>
type Queue<'V> = System.Collections.Generic.Queue<'V>
type GameId = System.Guid

module Storage =
    let private games = Dictionary<GameId, Game>()
    let private player1Results = Dictionary<GameId, Queue<IResult>>()
    let private player2Results = Dictionary<GameId, Queue<IResult>>()

    let updateGame (game: Game) : unit = games[ game.id ] <- game

    let createGame (game: Game) : unit =
        updateGame game
        player1Results.Add(game.id, Queue())
        player2Results.Add(game.id, Queue())

    let enqueueResult (result: GameResult) (game: Game) =
        // TODO make this more clear
        let go = GameState.details game.state
        let resultDto = GameResult.intoDto result go
        let recipient = GameResult.recipient result

        match recipient with
        | AllRecipients ->
            player1Results[ game.id ].Enqueue(resultDto)
            player2Results[ game.id ].Enqueue(resultDto)
        | PlayerRecipient player ->
            match player with
            | Player1 -> player1Results[ game.id ].Enqueue(resultDto)
            | Player2 -> player2Results[ game.id ].Enqueue(resultDto)

    let dequeResult (player: PlayerDto) (gameId: GameId) =
        let queue =
            match player with
            | PlayerDto.Player1 -> player1Results[gameId]
            | PlayerDto.Player2 -> player2Results[gameId]
            | _ -> System.ArgumentOutOfRangeException() |> raise

        if queue.Count = 0 then None else queue.Dequeue() |> Some

    let processMessage (player: PlayerDto) (gameId: GameId) (msg: IMessage) =
        let player = player |> GameMessage.fromPlayerDto
        let msg = msg |> GameMessage.fromDto player
        let game = games[gameId]
        let results, game = Game.update player msg game
        updateGame game
        List.iter (fun r -> enqueueResult r game) results

open Giraffe

let createGameApi (ctx: HttpContext) =
    { start =
        fun () ->
            async {
                let results, game = Game.newGame ()
                Storage.createGame game
                List.iter (fun r -> Storage.enqueueResult r game) results
                // See: https://stackoverflow.com/a/51255169/12347616
                ctx.GetLogger<IGameApi>()
                |> fun l -> l.LogInformation($"New Game [{game.id}] initialized")
                return (game.id.ToString(), PlayerDto.Player1)
            }

      poll =
          fun (id: string) (player: PlayerDto) ->
              async {
                  let id = System.Guid.Parse(id)
                  return Storage.dequeResult player id
              }

      update =
          fun (id: string) (player: PlayerDto) (msg: IMessage) ->
              async {
                  let id = System.Guid.Parse(id)
                  ctx.GetLogger<IGameApi>()
                  |> fun l -> l.LogInformation($"Update {msg}")
                  return Storage.processMessage player id msg
              } }

// See: https://github.com/Zaid-Ajaj/Fable.Remoting/blob/master/documentation/src/dependency-injection.md
let createApiFromContext (httpContext: HttpContext) : IGameApi =
    createGameApi httpContext

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext createApiFromContext
    |> Remoting.buildHttpHandler


// See: https://stackoverflow.com/a/51570113/12347616
let configureLogging (logging: ILoggingBuilder) =
     logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics*", LogLevel.None) |> ignore

let app =
    application {
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        logging configureLogging
    }

[<EntryPoint>]
let main _ =
    run app
    0
