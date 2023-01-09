module GameState



type GameStateUpdate = List<GameResult> * GameState

module PlayerOverseeState =
    let selectCharacter (p: Player) (c: CharacterId) (state: PlayerOversee) : GameStateUpdate =
        let character = state.details |> GameDetails.characters p |> Map.tryFind c

        // TODO: check if character was already moved
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

    let moveCharacter (player: Player) (pos: CellPosition) (state: PlayerMove) : GameStateUpdate =
        if not <| List.contains pos state.availableMoves then
            ([], PlayerMoveState(state))
        else
            let characterToMove = state.character
            let newBoard = state.details.board |> Board.moveCharacter characterToMove.id pos
            let details = { state.details with board = newBoard }

            // Look for all tiles in distance
            let boardPredicate (t: Tile) = true

            // Filter for actions that can be executed
            let boardActionExtractor (actionPredicate: Action.ApplicableToPredicate) (foundTiles: Board.FoundTiles) =
                foundTiles
                |> Board.FoundTiles.tiles
                |> List.map (fun t -> t |> Tile.characterId)
                |> List.choose id
                |> List.map (fun cid -> details |> GameDetails.fromCharacterId cid)
                |> List.filter (fun (c, p) -> actionPredicate p c)
                |> List.map (fun (c, _) -> c |> Character.id)

            // let boardActionPredicate (actionPredicate: Action.ApplicableToPredicate) (t: Tile) =
            //     t
            //     |> Tile.characterId
            //     |> Option.map (fun cid ->
            //         details |> GameDetails.fromCharacterId cid |> fun (p, c) -> actionPredicate c p)
            //     |> Option.defaultValue false
            //
            // let extract (foundTiles: Board.FoundTiles) =
            //     foundTiles
            //     |> Board.FoundTiles.tiles
            //     |> List.map Tile.characterId
            //     |> List.choose id

            let availableActions =
                state.character.actions
                |> List.map (fun action ->
                    let predicate = boardPredicate

                    let extract =
                        boardActionExtractor (Action.createApplicableToPredicate action player characterToMove)

                    let applicableCharacters =
                        newBoard |> Board.find pos action.distance predicate extract

                    if List.isEmpty applicableCharacters then
                        None
                    else
                        Some
                        <|

                        { action = action
                          selectableCharacters = applicableCharacters })
                |> List.choose id

            let msg =
                [ CharacterUpdate(state.character.id)
                  // Send action state select message
                  PlayerActionSelection(player, availableActions) ]

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
        let selectableAction =
            state.availableActions |> List.tryFind (fun a -> a.action.name = an)

        match selectableAction with
        | None -> ([], PlayerActionSelectState(state))
        | Some selectableAction ->

            // TODO: Send preview info when action is applied
            // E.g. after attack enemy has xyz hp
            let msg = [ PlayerAction(p, selectableAction.selectableCharacters) ]

            let state =
                PlayerActionState
                    { details = state.details
                      awaitingTurns = state.awaitingTurns
                      character = state.character
                      availableActions = state.availableActions
                      action = selectableAction }

            (msg, state)

    let update (msg: GameMessage) (state: PlayerActionSelect) : GameStateUpdate =
        match msg with
        | SelectAction (p, a) -> selectAction p a state

        | _ -> ([], PlayerActionSelectState(state))

module PlayerActionState =
    let deselectAction (p: Player) (state: PlayerAction) =
        let msg = [ PlayerActionSelection(p, state.availableActions) ]

        let state =
            PlayerActionSelectState
                { details = state.details
                  awaitingTurns = state.awaitingTurns
                  character = state.character
                  availableActions = state.availableActions }

        (msg, state)

    let rec performAction (player: Player) (cid: CharacterId) (state: PlayerAction) =
        let selectableAction = state.action
        let action = selectableAction.action
        let actionType = action.kind

        if not <| List.contains cid selectableAction.selectableCharacters then
            ([], PlayerActionState(state))
        else
            match actionType with
            | End ->
                let awaitingTurns = state.awaitingTurns |> Map.remove cid

                // TODO: check if awaitingTurns is empty

                let msg = [ PlayerOversee player ]

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
                          action = selectableAction }

                (msg, state)


    let update (msg: GameMessage) (state: PlayerAction) : GameStateUpdate =
        match msg with
        | DeselectAction p -> deselectAction p state
        | PerformAction (p, cid) -> performAction p cid state
        | _ -> ([], PlayerActionState(state))
