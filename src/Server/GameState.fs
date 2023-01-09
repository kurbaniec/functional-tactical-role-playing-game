module GameState

open Shared.DomainDto
open Utils

module GameDetails =
    let board (d: GameDetails) = d.board

    let characters (p: Player) (d: GameDetails) =
        match p with
        | Player1 -> d.player1Characters
        | Player2 -> d.player2Characters

    let fromCharacterId (cid: CharacterId) (game: GameDetails) : Character * Player =
        game.player1Characters
        |> Map.tryFind cid
        |> function
            | Some c -> (c, Player1)
            | None -> game.player2Characters |> Map.find cid |> fun c -> (c, Player2)

    let updateCharacter (c: Character) (p: Player) (d: GameDetails) : GameDetails =
        d.player1Characters
        |> Map.containsKey c.id
        |> function
            | true ->
                let p1c =
                    d.player1Characters
                    |> Map.change c.id (fun old ->
                        match old with
                        | Some _ -> Some c
                        | None -> None)

                { d with player1Characters = p1c }
            | false ->
                let p2c =
                    d.player2Characters
                    |> Map.change c.id (fun old ->
                        match old with
                        | Some _ -> Some c
                        | None -> None)

                { d with player2Characters = p2c }

    let removeCharacter (c: Character) (p: Player) (d: GameDetails) : GameDetails =
        let cid = c |> Character.id
        let board = d.board |> Board.removeCharacter cid

        match p with
        | Player1 ->
            let characters = d.player1Characters |> Map.remove cid

            { d with
                player1Characters = characters
                board = board }
        | Player2 ->
            let characters = d.player2Characters |> Map.remove cid

            { d with
                player2Characters = characters
                board = board }

    let isDefeated (p: Player) (d: GameDetails) : bool =
        match p with
        | Player1 -> d.player1Characters |> Map.isEmpty
        | Player2 -> d.player2Characters |> Map.isEmpty

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
        let msg = [ PlayerOversee ]

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

                    newBoard
                    |> Board.find pos action.distance predicate extract
                    |> fun cids ->
                        cids
                        |> List.isEmpty
                        |> function
                            | true -> None
                            | false ->
                                Some
                                <| { action = action
                                     selectableCharacters = cids })
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

    let performAction (player: Player) (cid: CharacterId) (state: PlayerAction) : GameStateUpdate =
        let thisCharacter = state.character
        let selectableAction = state.action
        let action = selectableAction.action
        let actionType = action.kind

        let oppositePlayer = player |> Player.opposite

        let validSelection (cid: CharacterId) =
            if List.contains cid selectableAction.selectableCharacters then
                Ok()
            else
                Error()

        let performAction () =
            let otherCharacter = state.details |> GameDetails.fromCharacterId cid |> fst

            Action.performAction thisCharacter otherCharacter action

        let actionWithoutChanges = ([], state.details)

        let actionWithChanges otherCharacter =
            let cid = otherCharacter |> Character.id

            if otherCharacter |> Character.isDefeated then
                state.details
                |> GameDetails.removeCharacter otherCharacter oppositePlayer
                |> fun details -> ([ CharacterDefeat cid ], details)
            else
                state.details
                |> GameDetails.updateCharacter otherCharacter oppositePlayer
                |> fun details -> ([ CharacterUpdate cid ], details)

        let actionChooser otherCharacter =
            match otherCharacter with
            | Some oc -> actionWithChanges oc
            | None -> actionWithoutChanges

        let stateCheck msgAndDetails =
            let (msg, details) = msgAndDetails

            if details |> GameDetails.isDefeated oppositePlayer then
                // Game End!
                match player with
                | Player1 -> (msg @ [ PlayerWin ], PlayerWinState details)
                | Player2 -> (msg @ [ PlayerWin ], PlayerWinState details)
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
                    let awaitingTurns = details |> GameDetails.characters player
                    let details = { details with turnOf = player }

                    let state =
                        PlayerOverseeState(
                            { details = details
                              awaitingTurns = awaitingTurns }
                        )

                    (msg @ [ PlayerOversee ], state)
                else
                    // Player still has characters to move
                    let state =
                        PlayerOverseeState(
                            { details = details
                              awaitingTurns = awaitingTurns }
                        )

                    (msg @ [ PlayerOversee ], state)


        cid
        |> validSelection
        |> Result.map performAction
        |> Result.map actionChooser
        |> Result.map stateCheck
        |> Result.defaultValue ([], PlayerActionState(state))




    let update (msg: GameMessage) (state: PlayerAction) : GameStateUpdate =
        match msg with
        | DeselectAction p -> deselectAction p state
        | PerformAction (p, cid) -> performAction p cid state
        | _ -> ([], PlayerActionState(state))
