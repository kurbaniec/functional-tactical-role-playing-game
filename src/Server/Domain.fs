[<AutoOpen>]
module Domain

open Shared.DomainDto

// open System

type Distance = Distance of int

module Distance =
    let value (Distance d) = d

type MovementType = Foot|Mount|Fly

module MovementType =
    let private footDistance: Lazy<Distance> = Lazy.Create(fun() -> Distance 2)
    let private mountDistance: Lazy<Distance> = Lazy.Create(fun() -> Distance 3)
    let private flyDistance: Lazy<Distance> = Lazy.Create(fun() -> Distance 3)
    let distance (m: MovementType) =
        match m with
        | Foot -> footDistance.Value
        | Mount -> mountDistance.Value
        | Fly -> flyDistance.Value

type Movement = {
    t: MovementType
    distance: Distance
}

type CharacterClass = Axe|Sword|Lance|Bow|Support
// type ActionValue = Damage of int|Heal of int
// let value (v: ActionValue) =
//     match v with
//     | Damage d -> d
//     | Heal h -> h
type ActionValue = ActionValue of int
let value (ActionValue v) = v

type ApplicableTo = Self|SelfAndAllies|Enemies





type Attack = {
    value: ActionValue
    distance: Distance
    applicable: ApplicableTo
}

type Heal = {
    value: ActionValue
    distance: Distance
    applicable: ApplicableTo
}

type Defend = {
    value: ActionValue
    distance: Distance
    applicable: ApplicableTo
}

type Action = Attack of Attack|Heal of Heal|Defend of Defend
type Actions = List<Action>

type CharacterId = System.Guid

type Character = {
    id: CharacterId
    name: string
    classification: CharacterClass
    actions: Actions
    movement: Movement
}

type Occupied = Option<CharacterId>
type Tile =
    |Land of Occupied
    |Water of Occupied

module Tile =
    let characterId (tile: Tile): Option<CharacterId> =
        match tile with
        | Land cid -> cid
        | Water cid -> cid

type Row = Row of int

module Row =
    let value (Row r) = r

type Col = Col of int

module Col =
    let value (Col c) = c
type CellPosition = Row * Col
type Board = Map<Row, Map<Col, Tile>>

type Characters = Map<CharacterId, Character>

type Player = Player1|Player2

type GameDetails = {
    turnOf: Player
    player1Characters: Characters
    player2Characters: Characters
    board: Board
}

module GameDetails =
    let board (d: GameDetails) = d.board
    let characters (p: Player) (d: GameDetails) =
        match p with
        | Player1 -> d.player1Characters
        | Player2 -> d.player2Characters

type Start = {
    rows: int
    cols: int
}

// type Cursor = {
//     row: int
//     col: int
// }

type PlayerOversee = {
    details: GameDetails
    awaitingTurns: Characters
}

type PlayerMove = {
    details: GameDetails
    awaitingTurns: Characters
    character: Character
}



type GameState =
    | PlayerOverseeState of PlayerOversee
    | PlayerMoveState of PlayerMove
    | PlayerActionState
    | Player1Wins
    | Player2Wins

module GameState =
    let details (gs: GameState): GameDetails =
        match gs with
        | PlayerOverseeState s -> s.details
        | PlayerMoveState s -> s.details
        | _ -> failwith "gamestate overview"

    let turnOf (gs: GameState): Player =
        gs |> details |> fun d -> d.turnOf

type GameId = System.Guid
type Game = {
    id: GameId
    state: GameState
}

type PlayerMoveResult =
    | CursorUpdate
    | ShowCharacterMovePath
    | MoveCharacter
    | HideCharacterMovePath
    | InvalidCharacterMovePath
    | ShowCharacterInfo
    | AllCharactersMoved

type PlayerAction = {
    state: GameDetails
    // cursor: Cursor
    // context menu ?
}



type GameResult =
    | Start of GameId
    | PlayerOversee of Player
    | PlayerMoveSelection of Player * CharacterId
    // | CursorUpdate
    // | ContextUpdate
    // | ShowCharacterActionPath
    // | HideCharacterActionPath
    // | ShowActionInfo

type GameMessage =
    | SelectCharacter of Player * CharacterId



module GameResult =
    type Recipient =
    | AllRecipients | PlayerRecipient of Player

    let recipient (gr: GameResult): Recipient =
        match gr with
        | Start _ -> AllRecipients
        | PlayerOversee _ -> AllRecipients
        | PlayerMoveSelection(p, _) -> PlayerRecipient p


