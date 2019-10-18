namespace RockPaperScissors


module Events =

    type GameId = string
    type PlayerId = string

    
    type PlayerEvent = 
    | PlayerCreated of PlayerCreated
    | PlayerNameChanged of PlayerNameChanged
    and PlayerCreated = {
        PlayerId: PlayerId
        Name: string
    }
    and PlayerNameChanged  = {
        PlayerId: PlayerId
        Name: string
    }

    type Shape = Rock | Paper | Scissors
    

    type GameEvent = 
    | GameCreated of GameCreated
    | PlayerJoined of PlayerJoined
    | PlayerLeft of PlayerLeft
    | GameStarted of GameStarted
    | PlayerPlayed of PlayerPlayed
    | GameEnded of GameEnded
    and GameCreated = {
        Id: GameId
        Host: PlayerId
    }
    and PlayerJoined = {
        GameId: GameId
        PlayerId: PlayerId
    }
    and PlayerLeft = {
        GameId: GameId
        PlayerId: PlayerId
    }
    and GameStarted = {
        Id: GameId
    }
    and PlayerPlayed = {
        GameId: GameId
        PlayerId: PlayerId
        Shape: Shape
    }
    and GameEnded = {
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
