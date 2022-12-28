module Domain





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

type ActionType = Axe|Sword|Lance|Bow|Heal
// type ActionValue = Damage of int|Heal of int
// let value (v: ActionValue) =
//     match v with
//     | Damage d -> d
//     | Heal h -> h
type ActionValue = ActionValue of int
let value (ActionValue v) = v


type Player = Player1|Player2

type Movement = {
    t: MovementType
    distance: Distance
}

type Action = {
    t: ActionType
    value: ActionValue
    distance: Distance
    applicable: Player
}

type Character = {
    name: string

}


