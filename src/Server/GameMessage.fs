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

let fromDto (p: Player) (msg: IMessage): GameMessage =
    match msg with
    | SelectCharacterDto c -> fromSelectCharacterDto p c
    | DeselectCharacterDto -> fromDeselectCharacterDto p
    | MoveCharacterDto pos -> fromMoveCharacterDto p pos
    | _ -> failwith "fromDto"

