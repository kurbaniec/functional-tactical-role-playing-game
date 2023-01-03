module GameMessage

open Shared.DomainDto

let fromPlayerDto (p: PlayerDto): Player =
    match p with
    | PlayerDto.Player1 -> Player.Player1
    | PlayerDto.Player2 -> Player.Player2

let fromSelectCharacterDto (p: Player) (c: string): GameMessage =
    let c = System.Guid.Parse(c)
    SelectCharacter (p, c)

let fromDto (p: Player) (msg: IMessage): GameMessage =
    match msg with
    | SelectCharacterDto c -> fromSelectCharacterDto p c
    | _ -> failwith "fromDto"

