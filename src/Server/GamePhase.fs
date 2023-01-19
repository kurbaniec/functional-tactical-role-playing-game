module GamePhase

open Utils

type GameStateUpdate = GameState.GameStateUpdate

module PlayerOverseePhase =
    let selectCharacter (p: Player) (c: CharacterId) (state: GameState) : GameStateUpdate =
        let character =
            state
            |> GameState.characters p
            |> Map.tryFind c
            |> Option.filter (fun c -> (state.awaitingTurns |> Map.containsKey c.id))

        match character with
        | None -> ([], state)
        | Some character ->
            let restoreState =
                { state = state
                  undoResults = [ PlayerOversee ] }

            let availableMoves = state |> GameState.board |> Board.availablePlayerMoves character

            printfn "Available moves"
            printfn $"%A{availableMoves}"

            let results = [ PlayerMoveSelection ]

            let phase =
                PlayerMovePhase
                    { character = character
                      availableMoves = availableMoves }

            let state =
                { state with
                    phase = phase
                    previous = Some restoreState }

            (results, state)


    let update (msg: GameMessage) (state: GameState) : GameStateUpdate =

        match msg with
        | SelectCharacter (p, c) -> selectCharacter p c state
        | _ -> state |> GameState.toEmptyUpdateWithMsg $"Unsupported message {msg}"

module PlayerMovePhase =
    let deselectCharacter (p: Player) (state: GameState) : GameStateUpdate = state |> GameState.toPreviousState

    let moveCharacter (player: Player) (pos: Position) (state: GameState) (phase: PlayerMove) : GameStateUpdate =
        if not <| List.contains pos phase.availableMoves then
            state |> GameState.toEmptyUpdate
        else
            let characterToMove = phase.character

            let restoreState =
                { state = state
                  undoResults = [ CharacterUpdate(characterToMove.id); PlayerOversee; PlayerMoveSelection ] }

            let board = state |> GameState.board |> Board.moveCharacter characterToMove.id pos
            let state = { state with board = board }


            // Filter for actions that can be executed
            // An action can be executed if it can be performed at least on one character

            // Look for all tiles in distance
            let boardPredicate (t: Tile) = true

            let boardActionExtractor (actionPredicate: Action.ApplicableToPredicate) (foundTiles: Board.FoundTiles) =
                foundTiles
                |> Board.FoundTiles.tiles
                |> List.map (fun t -> t |> Tile.characterId)
                |> List.choose id
                |> List.map (fun cid -> state |> GameState.fromCharacterId cid)
                |> List.filter (fun (c, p) -> actionPredicate p c)
                |> List.map (fun (c, _) -> c |> Character.id)

            let availableActions =
                characterToMove.actions
                |> List.map (fun action ->
                    let predicate = boardPredicate

                    let extract =
                        boardActionExtractor (Action.createApplicableToPredicate action player characterToMove)

                    board
                    |> Board.find pos action.distance predicate extract
                    |> fun cids ->
                        cids
                        |> List.isEmpty
                        |> function
                            | true -> None
                            | false ->
                                Some
                                <| { action = action
                                     selectableCharacters = cids
                                     preview = None })
                |> List.choose id

            let results =
                [ CharacterUpdate(characterToMove.id)
                  // Send action state select message
                  PlayerActionSelection ]

            let state =
                { state with
                    phase =
                        PlayerActionSelectPhase
                            { character = characterToMove
                              availableActions = availableActions }
                    previous = Some restoreState }

            (results, state)


    let update (msg: GameMessage) (state: GameState) (phase: PlayerMove) : GameStateUpdate =
        match msg with
        | DeselectCharacter (p) -> deselectCharacter p state
        | MoveCharacter (p, pos) -> moveCharacter p pos state phase
        | _ -> state |> GameState.toEmptyUpdateWithMsg $"Unsupported message {msg}"

module PlayerActionSelectPhase =
    let undoMoveCharacter (p: Player) (state: GameState) : GameStateUpdate = state |> GameState.toPreviousState

    let private precalculateAction
        (selectableAction: SelectableAction)
        (thisCharacter: Character)
        (gameState: GameState)
        =
        // Precalculate action result on all character it can be applied on
        selectableAction.selectableCharacters
        |> List.map (fun otherCharacterId ->
            let otherCharacter = gameState |> GameState.fromCharacterId otherCharacterId |> fst

            Action.performAction thisCharacter otherCharacter selectableAction.action
            |> function
                | None -> None
                | Some otherCharacterAfterAction -> Some(otherCharacterId, otherCharacterAfterAction))
        |> List.choose id
        |> fun charactersAfterAction ->
            if (charactersAfterAction |> List.isEmpty) then
                None
            else
                charactersAfterAction |> Map.ofList |> Some
        |> fun preview -> { selectableAction with preview = preview }

    let selectAction (player: Player) (actionName: ActionName) (state: GameState) (phase: PlayerActionSelect) =

        let selectableAction =
            phase.availableActions |> List.tryFind (fun a -> a.action.name = actionName)

        match selectableAction with
        | None -> state |> GameState.toEmptyUpdate
        | Some selectableAction ->
            let restoreState =
                { state = state
                  undoResults = [ PlayerActionSelection ] }

            let selectableAction = precalculateAction selectableAction phase.character state

            // E.g. after attack enemy has xyz hp
            let results = [ PlayerAction ]

            let phase =
                PlayerActionPhase
                    { character = phase.character
                      availableActions = phase.availableActions
                      action = selectableAction }

            let state =
                { state with
                    phase = phase
                    previous = Some restoreState }

            (results, state)

    let update (msg: GameMessage) (state: GameState) (phase: PlayerActionSelect) : GameStateUpdate =
        match msg with
        | UndoMoveCharacter p -> undoMoveCharacter p state
        | SelectAction (p, a) -> selectAction p a state phase
        | _ -> state |> GameState.toEmptyUpdateWithMsg $"Unsupported message {msg}"

module PlayerActionPhase =
    let deselectAction (p: Player) (state: GameState) = state |> GameState.toPreviousState

    let performAction (player: Player) (cid: CharacterId) (state: GameState) (phase: PlayerAction) : GameStateUpdate =
        let thisCharacter = phase.character
        let selectableAction = phase.action
        let actionPreview = selectableAction.preview

        let oppositePlayer = player |> Player.opposite

        let validSelection (cid: CharacterId) =
            if selectableAction.selectableCharacters |> List.contains cid then
                Ok()
            else
                Error()

        let applyAction () =
            // Return cached action result
            // Empty result means that action does not affect any character
            actionPreview
            |> Option.map (fun characters -> characters |> Map.tryFind cid)
            |> Option.flatten

        let actionWithoutChanges = state |> GameState.toEmptyUpdate

        let actionWithChanges otherCharacter =
            let cid = otherCharacter |> Character.id

            if otherCharacter |> Character.isDefeated then
                state
                |> GameState.removeCharacter otherCharacter oppositePlayer
                |> fun state -> ([ CharacterDefeat cid ], state)
            else
                state
                |> GameState.updateCharacter otherCharacter oppositePlayer
                |> fun state -> ([ CharacterUpdate cid ], state)

        let actionChooser otherCharacter =
            match otherCharacter with
            | Some oc -> actionWithChanges oc
            | None -> actionWithoutChanges

        let stateCheck resultsAndState =
            let results, state = resultsAndState

            if state |> GameState.isDefeated oppositePlayer then
                // Game End!
                match player with
                | Player1 -> (results @ [ PlayerWin ], { state with phase = PlayerWinPhase })
                | Player2 -> (results @ [ PlayerWin ], { state with phase = PlayerWinPhase })
            else
                let awaitingTurns =
                    state.awaitingTurns |> Map.remove (thisCharacter |> Character.id)

                if awaitingTurns |> Map.isEmpty then
                    let awaitingTurns = state |> GameState.characters oppositePlayer

                    let state =
                        { state with
                            phase = PlayerOverseePhase
                            turnOf = oppositePlayer
                            awaitingTurns = awaitingTurns
                            previous = None }

                    (results @ [ PlayerOversee ], state)
                else
                    // Player still has characters to move
                    let state =
                        { state with
                            phase = PlayerOverseePhase
                            awaitingTurns = awaitingTurns
                            previous = None }

                    (results @ [ PlayerOversee ], state)


        cid
        |> validSelection
        |> Result.map applyAction
        |> Result.map actionChooser
        |> Result.map stateCheck
        |> Result.defaultValue (state |> GameState.toEmptyUpdate)




    let update (msg: GameMessage) (state: GameState) (phase: PlayerAction) : GameStateUpdate =
        match msg with
        | DeselectAction p -> deselectAction p state
        | PerformAction (p, cid) -> performAction p cid state phase
        | _ -> state |> GameState.toEmptyUpdateWithMsg $"Unsupported message {msg}"
