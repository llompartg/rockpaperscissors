namespace RockPaperScissors

module Game =
    type GameId = GameId of string
    type PlayerId = PlayerId of string

    type Shape = Rock | Paper | Scissors

    type Command =
    | CreateGame of CreateGame
    | JoinGame of JoinGame
    | LeaveGame of LeaveGame
    | StartGame of StartGame
    | PlayShape of PlayShape
    and CreateGame = { Id: GameId; PlayerId: PlayerId; }
    and JoinGame = { Id: GameId; PlayerId: PlayerId}
    and LeaveGame = {Id: GameId; PlayerId: PlayerId}
    and StartGame = {Id: GameId}
    and PlayShape = {Id: GameId; PlayerId: PlayerId; Shape: Shape}

    type Event = 
    | GameCreated of GameCreated
    | PlayerJoined of PlayerJoined
    | PlayerLeft of PlayerLeft
    | GameStarted of GameStarted
    | PlayerPlayed of PlayerPlayed
    | GameEnded of GameEnded
    and GameCreated = {
        Id: GameId
        PlayerId: PlayerId
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
        GameId: GameId;
    }
    and PlayerPlayed = {
        GameId: GameId
        PlayerId: PlayerId
        Shape: Shape
    }
    and GameEnded = {
        Id: GameId
        Winner: PlayerId option
    }

    type Play = { PlayerId: PlayerId; Shape: Shape option}
    type GameState =
    | EmptyGame of EmptyGame
    | CreatedGame of CreatedGame
    | ReadyGame of ReadyGame
    | StartedGame of StartedGame
    | EndedGame of EndedGame
    and EmptyGame = {
        Id: GameId option
    }
    and CreatedGame = {
        Id: GameId;
        PlayerId: PlayerId
    }
    and ReadyGame = {
        Id: GameId;
        Players: PlayerId list;
    }
    and StartedGame = {
        Id: GameId;
        Plays: Play list
    }
    and EndedGame = {
        Id: GameId;
        Winner: PlayerId option
    }
    
    type Aggregate<'TState, 'TEvent, 'TCommand> ={
        zero: 'TState;
        apply: 'TState -> 'TEvent -> 'TState;
        exec: 'TState -> 'TCommand -> Result<'TEvent list, string>
    }

    let applyGameCreated (event: GameCreated): GameState = 
        { Id = event.Id; PlayerId = event.PlayerId} |> GameState.CreatedGame

    let applyPlayerJoined state (event: PlayerJoined): GameState =
        match state with
        | CreatedGame state ->
            let newPlayers = List.append [state.PlayerId] [event.PlayerId] |> List.distinct
            { Id = state.Id; Players = newPlayers; } |> GameState.ReadyGame
        | _ -> failwith (sprintf "What %A -> %A" state event)
    
    let applyPlayerLeft state (event: PlayerLeft) : GameState = 
        match state with
        | ReadyGame state -> 
            let remainingPlayer = 
                state.Players 
                |> List.filter (fun player -> player <> event.PlayerId)
                |> List.head
            { CreatedGame.Id = state.Id; PlayerId = remainingPlayer} |> GameState.CreatedGame
        | StartedGame state -> 
            let remainingPlayer = 
                state.Plays 
                |> List.filter (fun p -> p.PlayerId <> event.PlayerId)
                |> List.head
                |> (fun play -> play.PlayerId)
            
            { EndedGame.Id = state.Id; Winner = Some (remainingPlayer)} |> GameState.EndedGame
        | _ -> failwith (sprintf "What %A -> %A" state event)
    
    let applyGameStarted state event : GameState =
        match state with
        | ReadyGame game -> 
            let plays = game.Players |> List.map (fun p -> { PlayerId = p; Shape = None})
            { StartedGame.Id = game.Id; Plays = plays; } |> GameState.StartedGame
        | _ -> failwith (sprintf "What %A -> %A" state event)
    
    let applyPlayerPlayed state (event: PlayerPlayed) =
        match state with
        | StartedGame state ->
            let plays =
                state.Plays 
                |> List.map (fun p -> 
                if p.PlayerId = event.PlayerId then
                    { PlayerId = event.PlayerId; Shape = Some(event.Shape)}
                 else p
                 )
            { state with Plays = plays} |> GameState.StartedGame
        | _ -> failwith (sprintf "What %A -> %A" state event)

    let applyGameEnded state (event: GameEnded) =
        { Id = event.Id; Winner = event.Winner} |> GameState.EndedGame

    
    let apply state event =
        match event with
        | GameCreated event -> applyGameCreated event
        | PlayerJoined event -> applyPlayerJoined state event
        | PlayerLeft event -> applyPlayerLeft state event
        | GameStarted event -> applyGameStarted state event
        | PlayerPlayed event -> applyPlayerPlayed state event
        | GameEnded event -> applyGameEnded state event

    let execCreateGame state gameId playerId =
        match state with
        | EmptyGame _ -> Ok([Event.GameCreated { GameCreated.Id = gameId; PlayerId = playerId}])
        | _ -> Error("Game already exists")

    let execJoinGame state playerId =
        match state with
        | CreatedGame state -> 
            if state.PlayerId = playerId then Error("Player already in the game")
            else Ok([PlayerJoined { GameId = state.Id; PlayerId = playerId }])
        |  _ -> Error(sprintf "Player cannot join the game in this state %A" state)

    let execLeaveGame state playerId =
        match state with
        | CreatedGame state ->
            if state.PlayerId = playerId then
                [PlayerLeft { GameId = state.Id; PlayerId = playerId}; GameEnded { Id = state.Id; Winner = None}]
                |> Ok
            else Error("Player is not in this game")
        | ReadyGame state ->
            if List.contains playerId state.Players then
                [PlayerLeft { GameId = state.Id; PlayerId = playerId}] |> Ok
            else Error("Player is not in this game")
        | StartedGame state ->
            let canRemovePlayer = 
                state.Plays 
                |> List.map(fun p -> p.PlayerId) 
                |> List.contains playerId
            let remainingPlayer =
                state.Plays
                |> List.map(fun p -> p.PlayerId)
                |> List.filter(fun p -> p <> playerId)
                |> List.head
            if canRemovePlayer then
                [PlayerLeft { GameId = state.Id; PlayerId = playerId}; GameEnded { Id = state.Id; Winner = Some(remainingPlayer)}] 
                |> Ok
            else Error("Player is not in this game")
        | _ -> Error("Cannot leave game in current state")

    let execStartGame state gameId =
        match state with
        | ReadyGame _ -> [GameStarted { GameId = gameId }] |> Ok
        | _ -> Error("Unable to start game in given state")
        
    let execPlayShape state playerId shape =

        let pickWinner (plays: Play list) =
            let playerOne  = plays.[0]
            let playerTwo = plays.[1]
            
            match playerOne.Shape.Value, playerTwo.Shape.Value with
            | Rock, Rock -> None
            | Rock, Paper -> Some(playerTwo.PlayerId)
            | Rock, Scissors -> Some(playerOne.PlayerId)
            | Paper, Rock ->  Some(playerOne.PlayerId)
            | Paper, Paper -> None
            | Paper, Scissors -> Some(playerTwo.PlayerId)
            | Scissors, Rock ->  Some(playerTwo.PlayerId)
            | Scissors, Paper -> Some(playerOne.PlayerId)
            | Scissors, Scissors-> None

        match state with
        | StartedGame state -> 
            let play = 
                state.Plays 
                |> Seq.tryFind(fun x -> x.PlayerId = playerId) 
            match play with
            | Some(play)  -> 
                 match play.Shape with
                 | Some _ -> Error("Player has already played")
                 | None -> 
                    let updatedPlays = 
                        state.Plays 
                        |> List.map(fun play -> 
                            if play.PlayerId = playerId then 
                                { PlayerId = playerId; Shape = Some(shape)}
                            else play)
                    let isEndOfGame = 
                        updatedPlays 
                        |> List.choose(fun x -> x.Shape)
                        |> (fun m -> m.Length = 2)
                    let newPlayedEvent = PlayerPlayed { GameId = state.Id; PlayerId = playerId; Shape = shape}
                    match isEndOfGame with 
                    | true -> 
                        let endGameEvent = GameEnded { Id = state.Id; Winner = pickWinner updatedPlays}
                        (List.append [newPlayedEvent] [endGameEvent]) |> Ok
                    | false -> [newPlayedEvent] |> Ok
                    
            | None -> Error("Player is not in this game")
        | _ -> Error("Cannot play in given state")
        
    let exec state command : Result<Event list, string> = 
        match command with
        | CreateGame command -> execCreateGame state command.Id command.PlayerId
        | JoinGame command -> execJoinGame state command.PlayerId
        | LeaveGame command -> execLeaveGame state command.PlayerId
        | StartGame command -> execStartGame state command.Id
        | PlayShape command -> execPlayShape state command.PlayerId command.Shape

    let aggregate = {
        zero = EmptyGame { Id = None};
        apply = apply
        exec = exec
    }