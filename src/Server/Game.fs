﻿module Game

open GameState

let newGame (_: unit) : List<GameResult> * Game =
    let gid = System.Guid.NewGuid()
    let board = Board.create (Row 5) (Col 5)

    let playerPos = (Row 0, Col 0)
    let cid: CharacterId = System.Guid.NewGuid()
    let name = "Swordsman"
    let cls = CharacterClass.Sword

    let actions =
        [ Attack
              { value = ActionValue 1
                distance = Distance 1
                applicable = ApplicableTo.Enemies } ]

    let movement =
        { t = MovementType.Foot
          distance = Distance 2 }

    let character: Character =
        { id = cid
          name = name
          classification = cls
          actions = actions
          movement = movement }

    let characters = Map [ (character.id, character) ]

    let board = Board.placeCharacter playerPos character.id board

    let gameDetails =
        { turnOf = Player1
          player1Characters = characters
          player2Characters = Map []
          board = board }

    let playerOversee = {
        details = gameDetails
        awaitingTurns = gameDetails.player1Characters
    }

    let player1Oversee = PlayerOverseeState(playerOversee)

    ([ Start(gid); PlayerOversee(Player1) ], { id = gid; state = player1Oversee })

let isCorrectPlayer (player: Player) (game: Game) =
    game.state |> GameState.details |> fun d -> d.turnOf = player

let onPlayerMoveState
    (msg: GameMessage)
    (state: PlayerMove)
    : List<GameResult> * GameState =


    ([], PlayerMoveState(state))



let update (player: Player) (msg: GameMessage) (game: Game) : List<GameResult> * Game =
    // TODO rewrite as result type
    if not <| isCorrectPlayer player game then
        ([], game)
    else
        match game.state with
        | PlayerOverseeState s -> PlayerOverseeState.update msg s
        | PlayerMoveState s -> onPlayerMoveState msg s
        | _ -> failwith "update"
        |> fun (r, s) -> (r, { game with state = s })
