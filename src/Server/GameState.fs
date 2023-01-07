module GameState



type GameStateUpdate = List<GameResult> * GameState

module PlayerOverseeState =
    let selectCharacter (p: Player) (c: CharacterId) (state: PlayerOversee) : GameStateUpdate =
        let character = state.details |> GameDetails.characters p |> Map.tryFind c

        match character with
        | None -> ([], PlayerOverseeState(state))
        | Some c ->
            let availableMoves =
                state.details
                |> GameDetails.board
                |> Board.availablePlayerMoves c.movement.distance c.id

            printfn "Available moves"
            printfn $"%A{availableMoves}"

            let result = PlayerMoveSelection(p, c.id, availableMoves)

            let state =
                PlayerMoveState
                    { details = state.details
                      awaitingTurns = state.awaitingTurns
                      character = c
                      availableMoves = availableMoves }

            ([ result ], state)


    let update (msg: GameMessage) (state: PlayerOversee) : GameStateUpdate =

        match msg with
        | SelectCharacter (p, c) -> selectCharacter p c state
        | _ -> ([], PlayerOverseeState(state))

module PlayerMoveState =
    let deselectCharacter (p: Player) (state: PlayerMove) : GameStateUpdate =
        let msg = [ PlayerOversee p ]

        let state =
            PlayerOverseeState
                { details = state.details
                  awaitingTurns = state.awaitingTurns }

        (msg, state)

    let moveCharacter (p: Player) (pos: CellPosition) (state: PlayerMove) : GameStateUpdate =
        if not <| List.contains pos state.availableMoves then
            ([], PlayerMoveState(state))
        else
            let newBoard = state.details.board |> Board.moveCharacter state.character.id pos

            let details = { state.details with board = newBoard }

            // Filter for actions that can be executed
            let predicate (action: ApplicableTo) (t: Tile) =
                t
                |> Tile.characterId
                |> Option.map (fun cid -> details |> GameDetails.fromCharacterId cid |> fun (p, c) -> action c p)
                |> Option.defaultValue false

            let extract (foundTiles: Board.FoundTiles) =
                foundTiles
                |> Board.FoundTiles.tiles
                |> List.map Tile.characterId
                |> List.choose id

            let availableActions =
                state.character.actions
                |> List.map (fun a ->
                    let predicate = predicate a.applicableTo
                    let applicableCharacters = newBoard |> Board.find pos a.distance predicate extract

                    { action = a
                      applicableCharacters = applicableCharacters })

            let msg =
                [ CharacterUpdate(state.character.id)
                  // Send action state select message
                  PlayerActionSelection(p, availableActions) ]

            let state =
                PlayerActionSelectState
                    { details = details
                      awaitingTurns = state.awaitingTurns
                      character = state.character
                      availableActions = availableActions }

            (msg, state)


    let update (msg: GameMessage) (state: PlayerMove) : GameStateUpdate =
        match msg with
        | DeselectCharacter (p) -> deselectCharacter p state
        | MoveCharacter (p, pos) -> moveCharacter p pos state
        | _ -> ([], PlayerMoveState(state))

module PlayerActionSelectState =
    let selectAction (p: Player) (an: ActionName) (state: PlayerActionSelect) =
        let action =
            state.availableActions
            |> List.tryFind (fun a -> a.action.name = an)
            |> Option.map (fun a -> a.action)

        match action with
        | None -> ([], PlayerActionSelectState(state))
        | Some action ->
            match action.kind with
            | End ->
                // Dont require additional input
                let awaitingTurns = state.awaitingTurns |> Map.remove state.character.id

                // TODO: check if awaitingTurns is empty

                let msg = [ PlayerOversee p ]

                let state =
                    PlayerOverseeState
                        { details = state.details
                          awaitingTurns = awaitingTurns }

                (msg, state)
            | _ ->


                // TODO state result msg
                let msg = []
                // TODO perform action

                // TODO perform updates

                // TODO check win
                let state =
                    PlayerActionState
                        { details = state.details
                          awaitingTurns = state.awaitingTurns
                          character = state.character
                          availableActions = state.availableActions
                          action = action }

                (msg, state)

    let deselectAction (p: Player) (state: PlayerActionSelect) =
        let msg = [ PlayerActionSelection(p, state.availableActions) ]

        let state =
            PlayerActionSelectState
                { details = state.details
                  awaitingTurns = state.awaitingTurns
                  character = state.character
                  availableActions = state.availableActions }

        (msg, state)

    let update (msg: GameMessage) (state: PlayerActionSelect) : GameStateUpdate =
        match msg with
        | SelectAction (p, a) -> selectAction p a state
        | DeselectAction p -> deselectAction p state
        | _ -> ([], PlayerActionSelectState(state))
