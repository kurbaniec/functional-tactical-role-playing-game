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

let private newCharacterId() : CharacterId =
    System.Guid.NewGuid()
let private endTurnAction() =
    { name = "End"
      distance = Distance 0
      kind = End }

let createSwordWielder() =
    let attack = {
        name = "Sword Attack"
        distance = Distance 1
        kind = Attack
    }
    let movement = {
        kind = Foot
        distance = Distance 2
    }
    {
        id = newCharacterId()
        name = "Sword Wielder"
        stats = {
            hp = 12
            maxHp = 12
            def = 2
            atk = 8
            heal = 0
            cls = CharacterClass.Sword
        }
        actions = [attack; endTurnAction()]
        movement = movement
    }

let createLancer() =
    let attack = {
        name = "Lance Attack"
        distance = Distance 1
        kind = Attack
    }
    let movement = {
        kind = Mount
        distance = Distance 3
    }
    {
        id = newCharacterId()
        name = "Lancer"
        stats = {
            hp = 14
            maxHp = 14
            def = 3
            atk = 6
            heal = 0
            cls = CharacterClass.Lance
        }
        actions = [attack; endTurnAction()]
        movement = movement
    }

let createAxeMaster() =
    let attack = {
        name = "Axe Throw"
        distance = Distance 2
        kind = Attack
    }
    let movement = {
        kind = Fly
        distance = Distance 3
    }
    {
        id = newCharacterId()
        name = "Axe Master"
        stats = {
            hp = 10
            maxHp = 10
            def = 2
            atk = 7
            heal = 0
            cls = CharacterClass.Axe
        }
        actions = [attack; endTurnAction()]
        movement = movement
    }

let createHealer() =
    let attack = {
        name = "Wand Attack"
        distance = Distance 2
        kind = Attack
    }
    let heal = {
        name = "Wand Heal"
        distance = Distance 3
        kind = Heal
    }
    let movement = {
        kind = Foot
        distance = Distance 2
    }
    {
        id = newCharacterId()
        name = "Healer"
        stats = {
            hp = 8
            maxHp = 8
            def = 1
            atk = 4
            heal = 6
            cls = CharacterClass.Support
        }
        actions = [attack; heal; endTurnAction()]
        movement = movement
    }

let withNewId (character: Character) = { character with id = newCharacterId() }