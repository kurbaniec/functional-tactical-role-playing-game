module Character

let id (character: Character) = character.id

let def (character: Character) = character.stats.def
let hp (character: Character) = character.stats.hp
let maxHp (character: Character) = character.stats.maxHp
let atk (character: Character) = character.stats.atk
let heal (character: Character) = character.stats.heal

let movement (character: Character) = character.movement

let updateHp (amount: int) (character: Character): Character =
    character
    |> hp
    |> (+) amount
    |> fun hp -> System.Math.Clamp(hp, 0, character |> maxHp)
    |> fun hp -> { character.stats with hp = hp }
    |> fun stats -> { character with stats = stats }

let isDefeated (character: Character): bool = character.stats.hp <= 0