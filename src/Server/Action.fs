module Action


type OtherCharacterAfterAttack = Character
type ActionResult = Option<OtherCharacterAfterAttack>

module Attack =
    type WeaponTriangle =
        | Advantage
        | Normal
        | Disadvantage

    let weaponTriangle (thisCharacter: Character) (otherCharacter: Character) : WeaponTriangle =
        let cls = (thisCharacter.stats.cls, otherCharacter.stats.cls)

        match cls with
        | Sword, Axe -> WeaponTriangle.Advantage
        | Axe, Sword -> WeaponTriangle.Disadvantage
        | Axe, Lance -> WeaponTriangle.Advantage
        | Lance, Axe -> WeaponTriangle.Disadvantage
        | Lance, Sword -> WeaponTriangle.Advantage
        | Sword, Lance -> WeaponTriangle.Disadvantage
        | _ -> WeaponTriangle.Normal

    let attackValue (thisCharacter: Character) (otherCharacter: Character) (wt: WeaponTriangle) : int =
        let thisAtk = thisCharacter |> Character.atk
        let otherDef = otherCharacter |> Character.def

        match wt with
        | Advantage -> 1.25 * float (thisAtk - otherDef)
        | Disadvantage -> 0.75 * float (thisAtk - otherDef)
        | Normal -> float (thisAtk - otherDef)
        |> round
        |> int

    let attack (action: Action) (thisCharacter: Character) (otherCharacter: Character) : ActionResult =
        otherCharacter
        |> Character.updateHp (
            weaponTriangle thisCharacter otherCharacter
            |> attackValue thisCharacter otherCharacter
        )
        |> Some

module Heal =

    let rec healValue (thisCharacter: Character) (otherCharacter: Character) = thisCharacter |> Character.heal

    let heal (action: Action) (thisCharacter: Character) (otherCharacter: Character) : ActionResult =
        otherCharacter
        |> Character.updateHp (healValue thisCharacter otherCharacter)
        |> Some

let performAction (action: Action) (thisCharacter: Character) (otherCharacter: Character) : ActionResult =
    match action.kind with
    | End -> None
    | Attack -> Attack.attack action thisCharacter otherCharacter
    | Heal -> Heal.heal action thisCharacter otherCharacter

type ApplicableToPredicate = Player -> Character -> bool

let createApplicableToPredicate (action: Action) (player: Player) (character: Character) : ApplicableToPredicate =
    match action.kind with
    | End ->
        let endPredicate (characterOf: Player) (characterToCheck: Character) : bool = characterToCheck.id = character.id
        endPredicate
    | Attack ->
        let attackPredicate (characterOf: Player) (characterToCheck: Character) : bool = characterOf <> player
        attackPredicate
    | Heal ->
        let healPredicate (characterOf: Player) (characterToCheck: Character) : bool = characterOf = player
        healPredicate
