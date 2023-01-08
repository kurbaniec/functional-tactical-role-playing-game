module GameMessage

open Shared.DomainDto

let fromPlayerDto (p: PlayerDto): Player =
    match p with
    | PlayerDto.Player1 -> Player.Player1
    | PlayerDto.Player2 -> Player.Player2

let fromPositionDto (pos: PositionDto): CellPosition =
    (Row pos.row, Col pos.col)

let fromSelectCharacterDto (p: Player) (c: string): GameMessage =
    let c = System.Guid.Parse(c)
    SelectCharacter (p, c)

let fromDeselectCharacterDto (p: Player): GameMessage =
    DeselectCharacter p

let fromMoveCharacterDto (p: Player) (pos: PositionDto): GameMessage =
    MoveCharacter (p, fromPositionDto pos)

let fromSelectActionDto (p: Player) (actionName: string): GameMessage =
   SelectAction (p, ActionName actionName)

let fromDeselectActionDto (p: Player): GameMessage =
    DeselectAction (p)

let fromPerformActionDto (p: Player) (cid: string): GameMessage =
    PerformAction (p, cid |> System.Guid.Parse)

let fromDto (p: Player) (msg: IMessage): GameMessage =
    match msg with
    | SelectCharacterDto c -> fromSelectCharacterDto p c
    | DeselectCharacterDto -> fromDeselectCharacterDto p
    | MoveCharacterDto pos -> fromMoveCharacterDto p pos
    | SelectActionDto an -> fromSelectActionDto p an
    | DeselectActionDto -> fromDeselectActionDto p
    | PerformActionDto cid -> fromPerformActionDto p cid
    | _ -> failwith "fromDto"

