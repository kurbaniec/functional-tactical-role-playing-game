module Game

let newGame (_: unit) : List<GameResult> * Game =
    let gid = System.Guid.NewGuid()
    let board = Board.create (Row 5) (Col 5)

    let playerPos = (Row 0, Col 0)
    let cid: CharacterId = System.Guid.NewGuid()
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
        { id = cid
          name = name
          classification = cls
          actions = actions
          movement = movement }

    let characters = Map [ (character.id, character) ]

    let board = Board.placeCharacter playerPos character.id board

    let gameOverview =
        { player1Characters = characters
          player2Characters = Map []
          board = board }

    let playerOversee = { player = Player1 }
    let player1Oversee = Player1Oversee(playerOversee, gameOverview)

    ([ Start gid; PlayerOversee Player1 ], { id = gid; state = player1Oversee })
