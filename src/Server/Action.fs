module Action


type OtherCharacterAfterAction = Character
type ActionResult = Option<OtherCharacterAfterAction>

module ActionValue =
    let value (ActionValue v) = v

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
        |> fun damage -> System.Math.Clamp(damage, 0, System.Int32.MaxValue)
        |> (*) -1

    let attack (thisCharacter: Character) (otherCharacter: Character) (action: Action) : ActionResult =
        otherCharacter
        |> Character.updateHp (
            weaponTriangle thisCharacter otherCharacter
            |> attackValue thisCharacter otherCharacter
        )
        |> Some

module Heal =

    let rec healValue (thisCharacter: Character) (otherCharacter: Character) = thisCharacter |> Character.heal

    let heal (thisCharacter: Character) (otherCharacter: Character) (action: Action) : ActionResult =
        otherCharacter
        |> Character.updateHp (healValue thisCharacter otherCharacter)
        |> Some

let performAction (thisCharacter: Character) (otherCharacter: Character) (action: Action) : ActionResult =
    match action.kind with
    | End -> None
    | Attack -> Attack.attack thisCharacter otherCharacter action
    | Heal -> Heal.heal thisCharacter otherCharacter action

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
