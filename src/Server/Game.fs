module Game

open GameState

let newGame (gid: System.Guid) : List<GameResult> * Game =
    let board = Board.create (Row 7) (Col 7)

    let rnd = System.Random()
    let positionShuffle = [0; 1; -1]
    let rndOffset () = positionShuffle |> List.item (rnd.Next(positionShuffle |> List.length))
    let rowOffset = rndOffset()

    let mountains =
        [(1, 2); (1, 3); (1, 4); (2, 3)]
        |> List.map (fun pos -> (Tile.Mountain None, pos))

    let ponds =
        [(4, 2); (5, 4); (6, 3)]
        |> List.map (fun pos -> (Tile.Water None, pos))
        |> List.map (fun tile -> [tile])

    let obstacles =
        mountains :: ponds
        |> List.map (fun tileGroup ->
            let colOffset = rndOffset()
            tileGroup |> List.map (fun tile ->
                let tile, (row, col) = tile
                (tile, (Row (row + rowOffset), (Col (col + colOffset))))
            )
        )
        |> List.collect id

    let board =
        (board, obstacles)
        ||> List.fold (fun board (tile, pos) -> board |> Board.updateTile tile pos)



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
    let healerStats = { character.stats with heal = 3; cls = CharacterClass.Support }

    let healer =
        { character with
            id = healerId
            name = "Healer"
            actions = healActions
            stats = healerStats }

    let pos2 = (Row 2, Col 6)
    let character2 = {
        character with
            id = System.Guid.NewGuid()
            movement = { movement with kind = MovementType.Fly; distance=Distance 3  } }

    let characters =
        Map [ (character.id, character)
              (healer.id, healer) ]

    let characters2 = Map [ (character2.id, character2) ]

    let board =
        board
        |> Board.placeCharacter playerPos character.id
        |> Board.placeCharacter pos2 character2.id
        |> Board.placeCharacter healerPos healer.id


    // let gameDetails =
    //     { turnOf = Player1
    //       player1Characters = characters
    //       player2Characters = characters2
    //       board = board }
    //
    // let playerOversee =
    //     { details = gameDetails
    //       awaitingTurns = gameDetails.player1Characters }

    // let player1Oversee = PlayerOverseePhase(playerOversee)

    let phase = GamePhase.PlayerOverseePhase
    let state: GameState = {
         player1Characters = characters
         player2Characters = characters2
         board = board
         turnOf = Player1
         awaitingTurns = characters
         phase = phase
         previous = None
    }

    ([ Start(gid); PlayerOversee ], { id = gid; state = state })

let isCorrectPlayer (player: Player) (game: Game) =
    game.state |> GameState.turnOf |> (=) player

let state (game: Game) = game.state

let phase (game: Game) = game.state.phase

let update (player: Player) (msg: GameMessage) (game: Game) : List<GameResult> * Game =
    // TODO rewrite as result type
    if not <| isCorrectPlayer player game then
        ([], game)
    else
        let state = game |> state
        match game |> phase with
        | PlayerOverseePhase -> GamePhase.PlayerOverseePhase.update msg state
        | PlayerMovePhase phase -> GamePhase.PlayerMovePhase.update msg state phase
        | PlayerActionSelectPhase phase -> GamePhase.PlayerActionSelectPhase.update msg state phase
        | PlayerActionPhase phase -> GamePhase.PlayerActionPhase.update msg state phase
        | PlayerWinPhase -> state |> GameState.toEmptyUpdate // TODO: Send game already ended message
        |> fun (r, s) -> (r, { game with state = s })
