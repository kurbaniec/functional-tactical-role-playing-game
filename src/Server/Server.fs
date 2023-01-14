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

type ConcurrentDictionary<'K, 'V> = System.Collections.Concurrent.ConcurrentDictionary<'K, 'V>
type Dictionary<'K, 'V> = System.Collections.Generic.Dictionary<'K, 'V>
type Queue<'V> = System.Collections.Generic.Queue<'V>
type GameId = System.Guid

module GameCoordinator =
    let private join = Queue<GameId>()
    let private games = Dictionary<GameId, Game>()
    let private player1Results = Dictionary<GameId, Queue<IResult>>()
    let private player2Results = Dictionary<GameId, Queue<IResult>>()

    open Giraffe
    let logger (ctx: HttpContext) =
        // See: https://stackoverflow.com/a/51255169/12347616
        ctx.GetLogger<IGameApi>()

    let info (msg: string) (logger: ILogger) = logger.LogInformation(msg)

    let enqueueResult (result: GameResult) (game: Game) =
        // TODO make this more clear
        let gameState = game |> Game.state
        let resultDto = GameResult.intoDto result gameState
        let recipient = GameResult.recipient result

        match recipient with
        | AllRecipients ->
            player1Results[ game.id ].Enqueue(resultDto)
            player2Results[ game.id ].Enqueue(resultDto)
        | PlayerRecipient player ->
            match player with
            | Player1 -> player1Results[ game.id ].Enqueue(resultDto)
            | Player2 -> player2Results[ game.id ].Enqueue(resultDto)

    let dequeResult (player: PlayerDto) (gameId: GameId) (_: HttpContext) =
        let queue =
            match player with
            | PlayerDto.Player1 -> player1Results[gameId]
            | PlayerDto.Player2 -> player2Results[gameId]
            | _ -> System.ArgumentOutOfRangeException() |> raise

        if queue.Count = 0 then None else queue.Dequeue() |> Some

    let updateGame (game: Game) : unit = games[ game.id ] <- game

    let processMessage (player: PlayerDto) (gameId: GameId) (msg: IMessage) (ctx: HttpContext) =
        let logger = logger ctx
        logger |> info $"New message: {msg}"
        let player = player |> GameMessage.fromPlayerDto
        let msg = msg |> GameMessage.fromDto player
        let game = games[gameId]
        let results, game = Game.update player msg game
        logger |> info $"Update results: {results}"
        updateGame game
        List.iter (fun r -> enqueueResult r game) results

    let joinGame (ctx: HttpContext) : GameInfo =
        if join.Count = 0 then
            // No waiting players
            let gid = System.Guid.NewGuid()
            join.Enqueue(gid)
            // Create message queues
            player1Results.Add(gid, Queue())
            player2Results.Add(gid, Queue())
            logger ctx |> info $"New Game [{gid}] in join list"
            { id=gid.ToString(); player=PlayerDto.Player1 }
        else
            // Create game for joined players
            let gid = join.Dequeue()
            let results, game = Game.newGame gid
            updateGame game
            List.iter (fun r -> enqueueResult r game) results
            logger ctx |> info $"New Game [{gid}] created"
            { id=gid.ToString(); player=PlayerDto.Player2 }



let createGameApi (ctx: HttpContext) =
    { start =
        fun () -> async { return GameCoordinator.joinGame ctx }

      poll =
          fun (id: string) (player: PlayerDto) ->
              async {
                  let id = System.Guid.Parse(id)
                  return GameCoordinator.dequeResult player id ctx
              }

      update =
          fun (id: string) (player: PlayerDto) (msg: IMessage) ->
              async {
                  let id = System.Guid.Parse(id)
                  return GameCoordinator.processMessage player id msg ctx
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
