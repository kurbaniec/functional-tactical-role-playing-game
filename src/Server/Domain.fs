[<AutoOpen>]
module Domain

open Shared.DomainDto

// open System

type Distance = Distance of int

module Distance =
    let value (Distance d) = d


type MovementType =
    | Foot
    | Mount
    | Fly

type Movement = { kind: MovementType; distance: Distance }

type CharacterClass =
    | Axe
    | Sword
    | Lance
    | Bow
    | Support

type ActionValue = ActionValue of int
let value (ActionValue v) = v

type Player =
    | Player1
    | Player2

module Player =
    let opposite (player: Player) =
        match player with
        | Player1 -> Player2
        | Player2 -> Player1

type ActionName = string

type Action =
    { name: ActionName
      kind: ActionType
      distance: Distance
       }

// TODO: better name?
and SelectableAction =
    { action: Action
      selectableCharacters: list<CharacterId>
       }

and ActionType =
    | Attack
    | Heal
    | End

and Actions = List<Action>
and ApplicableActions = List<SelectableAction>

and CharacterId = System.Guid
and CharacterStats =
    {
        hp: int
        maxHp: int
        def: int
        atk: int
        heal: int
        cls: CharacterClass
    }

// TODO: add player field to streamline design
and Character =
    { id: CharacterId
      name: string
      stats: CharacterStats
      actions: Actions
      movement: Movement }

and Characters = Map<CharacterId, Character>

module Character =
    let id (character: Character) = character.id

    let def (character: Character) = character.stats.def
    let hp (character: Character) = character.stats.hp
    let maxHp (character: Character) = character.stats.maxHp
    let atk (character: Character) = character.stats.atk
    let heal (character: Character) = character.stats.heal

    let movement (character: Character) = character.movement

    let updateHp (amount: int) (character: Character): Character =
        character
        |> hp
        |> (+) amount
        |> fun hp -> System.Math.Clamp(hp, 0, character |> maxHp)
        |> fun hp -> { character.stats with hp = hp }
        |> fun stats -> { character with stats = stats }

    let isDefeated (character: Character): bool = character.stats.hp <= 0



type Occupied = Option<CharacterId>
module Occupied = Option

type Tile =
    | Land of Occupied
    | Water of Occupied

module Tile =
    let characterId (tile: Tile) : Occupied =
        match tile with
        | Land cid -> cid
        | Water cid -> cid

    let isOccupied (tile: Tile) : bool = tile |> characterId |> Option.isSome

    let occupy (cid: CharacterId) (tile: Tile) : Tile =
        match tile with
        | Land _ -> Tile.Land <| Some cid
        | Water _ -> Tile.Water <| Some cid

    let leave (tile: Tile) : Tile =
        match tile with
        | Land _ -> Tile.Land None
        | Water _ -> Tile.Water None


type Row = Row of int

module Row =
    let value (Row r) = r

type Col = Col of int

module Col =
    let value (Col c) = c

type CellPosition = Row * Col

module CellPosition =

    let value (c: CellPosition) =
        let (row, col) = c
        let row = row |> Row.value
        let col = col |> Col.value
        (row, col)

    let row (pos: CellPosition) = fst pos
    let col (pos: CellPosition) = snd pos

    let add (r: Row) (c: Col) (cell: CellPosition) : CellPosition =
        let row = cell |> row |> Row.value |> (+) <| Row.value r
        let col = cell |> col |> Col.value |> (+) <| Col.value c
        (Row row, Col col)


type Board = Map<Row, Map<Col, Tile>>



// type GameDetails =
//     { turnOf: Player
//       // TODO: change to characters: Map<Player, Map<cid, Character>?
//       player1Characters: Characters
//       player2Characters: Characters
//       board: Board }
//
// // TODO: move awaiting turns to GameDetails
// type PlayerOversee =
//     { details: GameDetails
//       awaitingTurns: Characters }
//
// type PlayerMove =
//     { details: GameDetails
//       awaitingTurns: Characters
//       character: Character
//       availableMoves: list<CellPosition> }
//
// type PlayerActionSelect =
//     { details: GameDetails
//       awaitingTurns: Characters
//       character: Character
//       availableActions: ApplicableActions }
//
// type PlayerAction =
//     { details: GameDetails
//       awaitingTurns: Characters
//       character: Character
//       availableActions: ApplicableActions
//       action: SelectableAction }
//
// // TODO: merge game details + state
// // details top level record prop with union state
// type GameState =
//     | PlayerOverseeState of PlayerOversee
//     | PlayerMoveState of PlayerMove
//     | PlayerActionSelectState of PlayerActionSelect
//     | PlayerActionState of PlayerAction
//     | PlayerWinState of GameDetails
//

type PlayerMove =
    { character: Character
      availableMoves: list<CellPosition> }

type PlayerActionSelect =
    { character: Character
      availableActions: ApplicableActions }

type PlayerAction =
    { character: Character
      availableActions: ApplicableActions
      action: SelectableAction }

type GamePhase =
    | PlayerOverseePhase
    | PlayerMovePhase of PlayerMove
    | PlayerActionSelectPhase of PlayerActionSelect
    | PlayerActionPhase of PlayerAction
    | PlayerWinPhase

type GameId = System.Guid

// TODO: Remove player?
type GameResult =
    | Start of GameId
    | PlayerOversee
    | PlayerMoveSelection of Player * CharacterId * list<CellPosition>
    | CharacterUpdate of CharacterId
    | PlayerActionSelection of Player * ApplicableActions
    | PlayerAction of Player * list<CharacterId>
    | CharacterDefeat of CharacterId
    | PlayerWin

type RestoreState = {
    state: GameState
    undoResults: list<GameResult>
}

and GameState = {
    player1Characters: Characters
    player2Characters: Characters
    board: Board
    turnOf: Player
    awaitingTurns: Characters
    phase: GamePhase
    previous: Option<RestoreState>
}


// module GameState =
//     let details (gs: GameState) : GameDetails =
//         match gs with
//         | PlayerOverseeState s -> s.details
//         | PlayerMoveState s -> s.details
//         | PlayerActionSelectState s -> s.details
//         | PlayerActionState s -> s.details
//         | PlayerWinState s -> s
//
//     let turnOf (gs: GameState) : Player = gs |> details |> fun d -> d.turnOf



type Game = { id: GameId; state: GameState }

// TODO move player out?
type GameMessage =
    | SelectCharacter of Player * CharacterId
    | DeselectCharacter of Player
    | MoveCharacter of Player * CellPosition
    | SelectAction of Player * ActionName
    | DeselectAction of Player
    | PerformAction of Player * CharacterId

// TODO: Make tuples?
// See: https://github.com/fsharp/fslang-suggestions/issues/743



module GameResult =
    type Recipient =
        | AllRecipients
        | PlayerRecipient of Player

    let recipient (gr: GameResult) : Recipient =
        match gr with
        | Start _ -> AllRecipients
        | PlayerOversee _ -> AllRecipients
        | PlayerMoveSelection (p, _, _) -> PlayerRecipient p
        | CharacterUpdate _ -> AllRecipients
        | PlayerActionSelection (p, _) -> PlayerRecipient p
        | PlayerAction (p, _) -> PlayerRecipient p
        | CharacterDefeat _ -> AllRecipients
        | PlayerWin _ -> AllRecipients
