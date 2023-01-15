module Player

let opposite (player: Player) =
        match player with
        | Player1 -> Player2
        | Player2 -> Player1