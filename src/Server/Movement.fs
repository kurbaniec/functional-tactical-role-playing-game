module Movement

module Distance =
    let value (Distance d) = d

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
        let flyPredicate (tile: Tile) =
            if tile |> Tile.isOccupied then
                false
            else
                match tile with
                | Mountain _ -> false
                | _ -> true
        flyPredicate
