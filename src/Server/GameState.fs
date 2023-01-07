﻿module GameState

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

    let moveCharacter (pos: CellPosition) (state: PlayerMove) : GameStateUpdate =
        if not <| List.contains pos state.availableMoves then
            ([], PlayerMoveState(state))
        else
            let newBoard =
                state.details.board
                |> Board.moveCharacter state.character.id pos
            let msg = [ CharacterUpdate(state.character.id) ]
            let details = { state.details with board = newBoard }

            // TODO: filter for actions that can be executed
            // distance check characters



            let predicate (action: ApplicableTo) (t: Tile) =
                t
                |> Tile.characterId
                |> Option.map (fun cid ->
                    details
                    |> GameDetails.fromCharacterId cid
                    |> fun (p, c) -> action c p
                   )
                |> Option.defaultValue false

            let availableAction =
                state.character.actions
                |> List.filter (fun a ->
                    let predicate = predicate a.applicableTo
                    newBoard
                    |> Board.containsCharacters pos a.distance predicate
                )




            let state =
                PlayerActionSelectState
                    { details = details
                      awaitingTurns = state.awaitingTurns
                      character = state.character
                      availableActions = state.character.actions }
            // TODO: send action state select message

            (msg, state)


    let update (msg: GameMessage) (state: PlayerMove) : GameStateUpdate =
        match msg with
        | DeselectCharacter (p) -> deselectCharacter p state
        | MoveCharacter (_, pos) -> moveCharacter pos state
        | _ -> ([], PlayerMoveState(state))

module PlayerActionSelectState =
    let selectAction (p: Player) (an: ActionName) (state: PlayerActionSelect) =
        let action = state.availableActions |> List.tryFind (fun a -> a.name = an)
        match action with
        | None -> ([], PlayerActionSelectState(state))
        | Some action ->

            // TODO msg
            let msg = []
            let state =
                PlayerActionState
                    { details = state.details
                      awaitingTurns = state.awaitingTurns
                      action = action
                       }

            (msg, state)




    let update (msg: GameMessage) (state: PlayerActionSelect) : GameStateUpdate =
        match msg with
        | SelectAction(p, a) -> selectAction p a state
        | _ -> ([], PlayerActionSelectState (state))