[<AutoOpen>]
module Domain

open Shared

//================================================================================
// Movement
//================================================================================
type Distance = Distance of int

type MovementType =
    | Foot
    | Mount
    | Fly

type Movement =
    { kind: MovementType
      distance: Distance }

//================================================================================
// Player
//================================================================================

type Player =
    | Player1
    | Player2

//================================================================================
// Character & Actions
//================================================================================

type CharacterId = System.Guid
type CharacterIds = list<CharacterId>

type CharacterClass =
    | Axe
    | Sword
    | Lance
    | Bow
    | Support

type ActionName = string

type ActionType =
    | Attack
    | Heal
    | End

type ActionValue = ActionValue of int

type Action =
    { name: ActionName
      kind: ActionType
      distance: Distance }

type Actions = List<Action>

type SelectableAction =
    { action: Action
      selectableCharacters: CharacterIds
      preview: option<Characters> }

and SelectableActions = list<SelectableAction>

and CharacterStats =
    { hp: int
      maxHp: int
      def: int
      atk: int
      heal: int
      cls: CharacterClass }

and Character =
    { id: CharacterId
      name: string
      stats: CharacterStats
      actions: Actions
      movement: Movement }

and Characters = Map<CharacterId, Character>

//================================================================================
// Board, Tiles & Position
//================================================================================

type Occupied = option<CharacterId>

type Tile =
    | Land of Occupied
    | Water of Occupied
    | Mountain of Occupied

type Row = Row of int
type Col = Col of int
type Position = Row * Col
type Board = Map<Row, Map<Col, Tile>>

//================================================================================
// Game, GameState & GamePhase
//================================================================================

type GameId = System.Guid

type PlayerMove =
    { character: Character
      availableMoves: list<Position> }

type PlayerActionSelect =
    { character: Character
      availableActions: SelectableActions }

type PlayerAction =
    { character: Character
      availableActions: SelectableActions
      action: SelectableAction }

type GamePhase =
    | PlayerOverseePhase
    | PlayerMovePhase of PlayerMove
    | PlayerActionSelectPhase of PlayerActionSelect
    | PlayerActionPhase of PlayerAction
    | PlayerWinPhase

type RestoreState =
    { state: GameState
      undoResults: list<GameResult> }

and GameState =
    { player1Characters: Characters
      player2Characters: Characters
      board: Board
      turnOf: Player
      awaitingTurns: Characters
      phase: GamePhase
      previous: Option<RestoreState> }

and Game = { id: GameId; state: GameState }

//================================================================================
// GameMessage & GameResults
//================================================================================

and GameResult =
    | Start of GameId
    | PlayerOversee
    | PlayerMoveSelection
    | CharacterUpdate of CharacterId
    | PlayerActionSelection
    | PlayerAction
    | CharacterDefeat of CharacterId
    | PlayerWin
    | Unsupported of DomainDto.ErrorMsg

type Recipient =
    | AllRecipients
    | PlayerRecipient of Player

type GameMessage =
    | SelectCharacter of Player * CharacterId
    | DeselectCharacter of Player
    | MoveCharacter of Player * Position
    | UndoMoveCharacter of Player
    | SelectAction of Player * ActionName
    | DeselectAction of Player
    | PerformAction of Player * CharacterId


