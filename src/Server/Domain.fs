module Domain

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

type CharacterClass = Axe|Sword|Lance|Bow|Heal
// type ActionValue = Damage of int|Heal of int
// let value (v: ActionValue) =
//     match v with
//     | Damage d -> d
//     | Heal h -> h
type ActionValue = ActionValue of int
let value (ActionValue v) = v

type ApplicableTo = Self|SelfAndAllies|Enemies


type Player = Player1|Player2


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

type CharacterId = Guid


// TODO: Action / Context
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

type Row = Row of int
type Col = Col of int
type CellPosition = Row * Col
type Board = Map<Row, Map<Col, Tile>>

type GamePhase =
    | Start
    | Player1Move
    | Player1Action
    | Player2Move
    | Player2Action
    | Player1Wins
    | Player2Wins


type Characters = Map<CharacterId, Character>

type GameState = {
    characters: Characters
    board: Board
}



type PlayerMove = {
    state: GameState
    // cursor: Cursor | CursorPos
    // character: Option<CharacterId>
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
    state: GameState
    // cursor: Cursor
    // context menu ?
}

type PlacerActionResult =
    | CursorUpdate
    | ContextUpdate
    | ShowCharacterActionPath
    | HideCharacterActionPath
    | ShowActionInfo





