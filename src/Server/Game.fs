﻿module Game

open GameState

let newGame (_: unit) : List<GameResult> * Game =
    let gid = System.Guid.NewGuid()
    let board = Board.create (Row 5) (Col 5)

    let playerPos = (Row 0, Col 0)
    let cid: CharacterId = System.Guid.NewGuid()
    let name = "Swordsman"
    let cls = CharacterClass.Sword

    let endTurn =
        { name = "End"
          distance = Distance 0
          kind = End }

    let actions =
        [ { name = "Attack"
            distance = Distance 1
            kind = Attack }
          endTurn ]

    let movement =
        { kind = MovementType.Foot
          distance = Distance 2 }

    let character: Character =
        { id = cid
          name = name
          stats =
            { hp = 1
              maxHp = 10
              def = 10
              atk = 5
              heal = 0
              cls = cls }
          actions = actions
          movement = movement }

    let healerPos = (Row 3, Col 0)
    let healerId = System.Guid.NewGuid()

    let heal =
        { name = "Heal"
          distance = Distance 2
          kind = Heal }

    let healActions = heal :: actions
    let healerStats = { character.stats with heal = 3 }

    let healer =
        { character with
            id = healerId
            name = "Healer"
            actions = healActions
            stats = healerStats }

    let pos2 = (Row 2, Col 4)
    let character2 = { character with id = System.Guid.NewGuid() }

    let characters =
        Map [ (character.id, character)
              (healer.id, healer) ]

    let characters2 = Map [ (character2.id, character2) ]

    let board =
        board
        |> Board.placeCharacter playerPos character.id
        |> Board.placeCharacter pos2 character2.id
        |> Board.placeCharacter healerPos healer.id


    let gameDetails =
        { turnOf = Player1
          player1Characters = characters
          player2Characters = characters2
          board = board }

    let playerOversee =
        { details = gameDetails
          awaitingTurns = gameDetails.player1Characters }

    let player1Oversee = PlayerOverseeState(playerOversee)

    ([ Start(gid); PlayerOversee ], { id = gid; state = player1Oversee })

let isCorrectPlayer (player: Player) (game: Game) =
    game.state |> GameState.details |> fun d -> d.turnOf = player


let update (player: Player) (msg: GameMessage) (game: Game) : List<GameResult> * Game =
    // TODO rewrite as result type
    if not <| isCorrectPlayer player game then
        ([], game)
    else
        match game.state with
        | PlayerOverseeState s -> PlayerOverseeState.update msg s
        | PlayerMoveState s -> PlayerMoveState.update msg s
        | PlayerActionSelectState s -> PlayerActionSelectState.update msg s
        | PlayerActionState s -> PlayerActionState.update msg s
        | PlayerWinState s -> ([], PlayerWinState s) // TODO: Send game already ended message
        |> fun (r, s) -> (r, { game with state = s })
