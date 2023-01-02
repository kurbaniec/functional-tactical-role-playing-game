module Game

let newGame (_: unit) : List<GameResult> * Game =
    let board = Board.create (Row 5) (Col 5)

    let playerPos = (Row 0, Col 0)
    let id: CharacterId = System.Guid.NewGuid()
    let name = "Swordsman"
    let cls = CharacterClass.Sword

    let actions =
        [ Attack
              { value = ActionValue 1
                distance = Distance 1
                applicable = ApplicableTo.Enemies } ]

    let movement =
        { t = MovementType.Foot
          distance = Distance 2 }

    let character: Character =
        { id = id
          name = name
          classification = cls
          actions = actions
          movement = movement }

    let characters = Map [ (character.id, character) ]

    let board = Board.placeCharacter playerPos character board

    let gameOverview =
        { characters = characters
          board = board }

    let playerOversee = { player = Player1 }
    let player1Oversee = Player1Oversee(playerOversee, gameOverview)

    ([ Start; PlayerOversee ], { state = player1Oversee })
