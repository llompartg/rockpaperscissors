module Tests

open System
open Xunit

open RockPaperScissors.Game

[<Fact>]
let ``My test`` () =
    let load gameId =
        let events: Event list = [ 
            Event.GameCreated { Id = gameId; PlayerId = PlayerId "John" }; 
            Event.PlayerJoined { GameId = gameId; PlayerId = PlayerId "Mike" }; 
            Event.PlayerLeft { GameId = gameId; PlayerId = PlayerId "Mike" }; 
            Event.PlayerJoined{ GameId = gameId; PlayerId = PlayerId "Peter"}; 
            Event.GameStarted { GameId = gameId;}; 
            Event.PlayerPlayed{ GameId = gameId; PlayerId = PlayerId "Peter"; Shape = Rock}; 
            Event.PlayerPlayed{ GameId = gameId; PlayerId = PlayerId "John"; Shape = Paper}; 
            Event.GameEnded{ Id = gameId ; Winner = Some (PlayerId "John")}]
        events
    
    let gameId = GameId "1234"
    
    let events = load gameId
    let result = Seq.fold aggregate.apply aggregate.zero events
    Assert.True(true)

[<Fact>]
let ``A game can be created``() =
    let command = Command.CreateGame {Id = GameId "12345"; PlayerId = PlayerId "John"}
    let result = aggregate.exec aggregate.zero command
    failwith (sprintf "%A" result)
    Assert.True(true)

[<Fact>]
let ``Players can join available games``() =
    let gameId = GameId "1234"
    let events = [Event.GameCreated { Id = gameId; PlayerId = PlayerId "John" }]; 

    let state = Seq.fold aggregate.apply aggregate.zero events 

    let command = Command.JoinGame {Id = GameId "1234"; PlayerId = PlayerId "Mike"}
    let result = aggregate.exec state command
    failwith (sprintf "%A" result)
    Assert.True(true)