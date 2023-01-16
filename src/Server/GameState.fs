module GameState

open Utils

type GameStateUpdate = List<GameResult> * GameState

let player1Characters (gameState: GameState) = gameState.player1Characters
let player2Characters (gameState: GameState) = gameState.player2Characters
let board (gameState: GameState) = gameState.board
let turnOf (gameState: GameState) = gameState.turnOf
let awaitingTurns (gameState: GameState) = gameState.awaitingTurns
let phase (gameState: GameState) = gameState.phase
let previous (gameState: GameState) = gameState.previous

let characters (p: Player) (gameState: GameState) =
    match p with
    | Player1 -> gameState.player1Characters
    | Player2 -> gameState.player2Characters

let fromCharacterId (cid: CharacterId) (gameState: GameState) : Character * Player =
    gameState.player1Characters
    |> Map.tryFind cid
    |> function
        | Some c -> (c, Player1)
        | None -> gameState.player2Characters |> Map.find cid |> fun c -> (c, Player2)

let updateCharacter (c: Character) (p: Player) (gameState: GameState) : GameState =
    gameState.player1Characters
    |> Map.containsKey c.id
    |> function
        | true ->
            let p1c =
                gameState.player1Characters
                |> Map.change c.id (fun old ->
                    match old with
                    | Some _ -> Some c
                    | None -> None)

            { gameState with player1Characters = p1c }
        | false ->
            let p2c =
                gameState.player2Characters
                |> Map.change c.id (fun old ->
                    match old with
                    | Some _ -> Some c
                    | None -> None)

            { gameState with player2Characters = p2c }

let removeCharacter (c: Character) (p: Player) (gameState: GameState) : GameState =
    let cid = c |> Character.id
    let board = gameState.board |> Board.removeCharacter cid

    match p with
    | Player1 ->
        let characters = gameState.player1Characters |> Map.remove cid

        { gameState with
            player1Characters = characters
            board = board }
    | Player2 ->
        let characters = gameState.player2Characters |> Map.remove cid

        { gameState with
            player2Characters = characters
            board = board }

let isDefeated (p: Player) (gameState: GameState) : bool =
    match p with
    | Player1 -> gameState.player1Characters |> Map.isEmpty
    | Player2 -> gameState.player2Characters |> Map.isEmpty

let toEmptyUpdate (gameState: GameState) : GameStateUpdate = ([], gameState)

let toEmptyUpdateWithMsg (msg: string) (gameState: GameState) : GameStateUpdate
    = ([Unsupported msg], gameState)

let toPreviousState (gameState: GameState) : GameStateUpdate =
    match gameState |> previous with
    | None -> gameState |> toEmptyUpdate
    | Some previous -> (previous.undoResults, previous.state)

