module GameState

type GameStateUpdate = List<GameResult> * GameState

module PlayerOverseeState =
    let selectCharacter (p: Player) (c: CharacterId) (state: PlayerOversee) : GameStateUpdate =
        let character = state.details |> GameDetails.characters p |> Map.tryFind c

        match character with
        | None -> ([], PlayerOverseeState(state))
        | Some c ->
            let result = PlayerMoveSelection(p, c.id)

            let state =
                PlayerMoveState
                    { details = state.details
                      awaitingTurns = state.awaitingTurns
                      character = c }

            ([ result ], state)


    let update (msg: GameMessage) (state: PlayerOversee) : GameStateUpdate =

        match msg with
        | SelectCharacter (p, c) -> selectCharacter p c state
        | _ -> ([], PlayerOverseeState(state))
