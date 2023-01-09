module Action

type OtherCharacterAfterAttack = Character
type ActionResult = Option<OtherCharacterAfterAttack>

let attack (action: Action) (thisCharacter: Character) (otherCharacter): ActionResult =
    // TODO attack character
    // TODO weapon triangle
    Some otherCharacter

let heal (action: Action) (thisCharacter: Character) (otherCharacter): ActionResult =
    // TODO heal character
    Some otherCharacter

let performAction (action: Action) (thisCharacter: Character) (otherCharacter): ActionResult =
    match action.kind with
    | End -> None
    | Attack -> attack action thisCharacter otherCharacter
    | Heal -> heal action thisCharacter otherCharacter

type ApplicableToPredicate = Player -> Character -> bool
let createApplicableToPredicate
    (action: Action)
    (player: Player)
    (character: Character)
    : ApplicableToPredicate =
    match action.kind with
    | End ->
        let endPredicate (characterOf: Player) (characterToCheck: Character): bool =
            characterToCheck.id = character.id
        endPredicate
    | Attack ->
        let attackPredicate (characterOf: Player) (characterToCheck: Character): bool =
            characterOf <> player
        attackPredicate
    | Heal ->
        let healPredicate (characterOf: Player) (characterToCheck: Character): bool =
            characterOf = player
        healPredicate