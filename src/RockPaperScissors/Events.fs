namespace RockPaperScissors


module Events =

    type GameId = string
    type PlayerId = string

    type PlayerCreated = {
        PlayerId: PlayerId
        Name: string
    }
    type PlayerNameChanged  = {
        PlayerId: PlayerId
        Name: string
    }

    type Shape = Rock | Paper | Scissors
    
    type PlayerJoined = {
        GameId: GameId
        PlayerId: PlayerId
    }

    type PlayerLeft = {
        GameId: GameId
        PlayerId: PlayerId
    }

    type GameCreated = {
        Id: GameId
        Host: PlayerId
    }

    type GameStarted = {
        Id: GameId
    }

    type PlayerPlayed = {
        GameId: GameId
        PlayerId: PlayerId
        Shape: Shape
    }
    type GameEnded = {
        Id: GameId
        Winner: PlayerId
    }

    let e1: GameCreated =  { Id = "1234"; Host = "John" }
    let e2: PlayerJoined = { GameId = "1234"; PlayerId = "Mike" }
    let e3: PlayerLeft = { GameId = "1234"; PlayerId = "Mike" }
    let e4: PlayerJoined = { GameId = "1234"; PlayerId = "Peter"}
    let e5: GameStarted = { Id = "1234" }
    let e6: PlayerPlayed = { GameId = "1234"; PlayerId = "Peter"; Shape = Rock}
    let e7: PlayerPlayed = { GameId = "1234"; PlayerId = "John"; Shape = Paper}
    let e8: GameEnded = { Id = "1234"; Winner = "John"}
