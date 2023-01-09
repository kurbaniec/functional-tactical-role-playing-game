module Movement

module Type =
    let private footDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 2)
    let private mountDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 3)
    let private flyDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 3)

    let distance (m: MovementType) =
        match m with
        | Foot -> footDistance.Value
        | Mount -> mountDistance.Value
        | Fly -> flyDistance.Value

type MovementPredicate = Tile -> bool

let createMovementPredicate (character: Character) =
    let movement = character |> Character.movement

    match movement.kind with
    | Foot
    | Mount ->
        let landPredicate (tile: Tile) =
            if tile |> Tile.isOccupied then
                false
            else
                match tile with
                | Land _ -> true
                | _ -> false

        landPredicate
    | Fly ->
        let flyPredicate (tile: Tile) = tile |> Tile.isOccupied |> not
        flyPredicate
