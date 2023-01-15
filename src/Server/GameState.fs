module GameState

open Shared.DomainDto
open Utils

type GameStateUpdate = List<GameResult> * GameState

module GameState =

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

    let toPreviousState (gameState: GameState) : GameStateUpdate =
        match gameState |> previous with
        | None -> gameState |> toEmptyUpdate
        | Some previous -> (previous.undoResults, previous.state)


module PlayerOverseePhase =
    let selectCharacter (p: Player) (c: CharacterId) (state: GameState) : GameStateUpdate =
        let character =
            state
            |> GameState.characters p
            |> Map.tryFind c
            |> Option.filter (fun c -> (state.awaitingTurns |> Map.containsKey c.id))

        match character with
        | None -> ([], state)
        | Some c ->
            let restoreState = {
                state = state
                undoResults = [ PlayerOversee ]
            }

            let availableMoves = state |> GameState.board |> Board.availablePlayerMoves c

            printfn "Available moves"
            printfn $"%A{availableMoves}"

            let result = PlayerMoveSelection(p, c.id, availableMoves)

            let phase =
                PlayerMovePhase
                    { character = c
                      availableMoves = availableMoves }

            let state =
                { state with
                    phase = phase
                    previous = Some restoreState }

            ([ result ], state)


    let update (msg: GameMessage) (state: GameState) : GameStateUpdate =

        match msg with
        | SelectCharacter (p, c) -> selectCharacter p c state
        | _ -> state |> GameState.toEmptyUpdate // TODO: send unsupported msg result

module PlayerMoveState =
    let deselectCharacter (p: Player) (state: GameState) : GameStateUpdate = state |> GameState.toPreviousState

    let moveCharacter (player: Player) (pos: Position) (state: GameState) (phase: PlayerMove) : GameStateUpdate =
        if not <| List.contains pos phase.availableMoves then
            state |> GameState.toEmptyUpdate
        else
            let characterToMove = phase.character

            let restoreState =
                { state = state
                  undoResults = [ CharacterUpdate(characterToMove.id); PlayerOversee ] }

            let board = state |> GameState.board |> Board.moveCharacter characterToMove.id pos
            let state = { state with board = board }

            // Look for all tiles in distance
            let boardPredicate (t: Tile) = true
            // TODO: Move extraction logic to board module?
            // Filter for actions that can be executed
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

            let msg =
                [ CharacterUpdate(characterToMove.id)
                  // Send action state select message
                  PlayerActionSelection(player, availableActions) ]

            let state =
                { state with
                    phase =
                        PlayerActionSelectPhase
                            { character = characterToMove
                              availableActions = availableActions }
                    previous = Some restoreState }

            (msg, state)


    let update (msg: GameMessage) (state: GameState) (phase: PlayerMove) : GameStateUpdate =
        match msg with
        | DeselectCharacter (p) -> deselectCharacter p state
        | MoveCharacter (p, pos) -> moveCharacter p pos state phase
        | _ -> state |> GameState.toEmptyUpdate

module PlayerActionSelectPhase =
    let selectAction (player: Player) (actionName: ActionName) (state: GameState) (phase: PlayerActionSelect) =
        let selectableAction =
            phase.availableActions |> List.tryFind (fun a -> a.action.name = actionName)

        match selectableAction with
        | None -> state |> GameState.toEmptyUpdate
        | Some selectableAction ->
            let restoreState =
                { state = state
                  undoResults = [ PlayerActionSelection(player, phase.availableActions) ] }

            // TODO: Send preview info when action is applied
            // E.g. after attack enemy has xyz hp
            let msg = [ PlayerAction(player, selectableAction.selectableCharacters) ]

            let phase =
                PlayerActionPhase
                    { character = phase.character
                      availableActions = phase.availableActions
                      action = selectableAction }

            let state =
                { state with
                    phase = phase
                    previous = Some restoreState }

            (msg, state)

    let update (msg: GameMessage) (state: GameState) (phase: PlayerActionSelect) : GameStateUpdate =
        match msg with
        | SelectAction (p, a) -> selectAction p a state phase
        | _ -> state |> GameState.toEmptyUpdate

module PlayerActionPhase =
    let deselectAction (p: Player) (state: GameState) =
        // let msg = [ PlayerActionSelection(p, state.availableActions) ]
        //
        // let state =
        //     PlayerActionSelectPhase
        //         { details = state.details
        //           awaitingTurns = state.awaitingTurns
        //           character = state.character
        //           availableActions = state.availableActions }
        //
        // (msg, state)
        state |> GameState.toPreviousState

    let performAction (player: Player) (cid: CharacterId) (state: GameState) (phase: PlayerAction) : GameStateUpdate =
        let thisCharacter = phase.character
        let selectableAction = phase.action
        let action = selectableAction.action
        let actionType = action.kind

        let oppositePlayer = player |> Player.opposite

        let validSelection (cid: CharacterId) =
            if List.contains cid selectableAction.selectableCharacters then
                Ok()
            else
                Error()

        let performAction () =
            let otherCharacter = state |> GameState.fromCharacterId cid |> fst

            Action.performAction thisCharacter otherCharacter action

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

        let stateCheck msgAndState =
            let (msg, state) = msgAndState

            if state |> GameState.isDefeated oppositePlayer then
                // Game End!
                match player with
                | Player1 -> (msg @ [ PlayerWin ], { state with phase = PlayerWinPhase })
                | Player2 -> (msg @ [ PlayerWin ], { state with phase = PlayerWinPhase })
            else
                // TODO: when awaiting turns is merge to GameDetails make if else
                let awaitingTurns =
                    state.awaitingTurns |> Map.remove (thisCharacter |> Character.id)

                if awaitingTurns |> Map.isEmpty then
                    // Player turn switch
                    // TODO: change when implementing second player
                    (*let awaitingTurns = details |> GameDetails.characters oppositePlayer
                    let details = { details with turnOf = oppositePlayer }

                    let state =
                        PlayerOverseeState(
                            { details = details
                              awaitingTurns = awaitingTurns }
                        )

                    (msg @ [ PlayerOversee oppositePlayer ], state)*)
                    let awaitingTurns = state |> GameState.characters oppositePlayer

                    let state =
                        { state with
                            phase = PlayerOverseePhase
                            turnOf = oppositePlayer
                            awaitingTurns = awaitingTurns
                            previous = None }

                    (msg @ [ PlayerOversee ], state)
                else
                    // Player still has characters to move
                    let state =
                        { state with
                            phase = PlayerOverseePhase
                            awaitingTurns = awaitingTurns
                            previous = None }

                    (msg @ [ PlayerOversee ], state)


        cid
        |> validSelection
        |> Result.map performAction
        |> Result.map actionChooser
        |> Result.map stateCheck
        |> Result.defaultValue (state |> GameState.toEmptyUpdate)




    let update (msg: GameMessage) (state: GameState) (phase: PlayerAction) : GameStateUpdate =
        match msg with
        | DeselectAction p -> deselectAction p state
        | PerformAction (p, cid) -> performAction p cid state phase
        | _ -> state |> GameState.toEmptyUpdate
