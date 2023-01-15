module Tile

module Occupied = Option

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

