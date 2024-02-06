module GameState

open Field

[<RequireQualifiedAccess>]
type BotType =
    | CheezyBot
    | StrategicBot
    | NerdBot

type Player =
    { Index: int
      Id: string
      Name: string
      DiceBuffer: int
      DiceMovesThisTurn: int
      ContinuousFieldCount: int
      IsReady: bool
      BotType: int option }

type Attack =
    { AttackerId: string
      AttackingFieldId: string
      AttackingDiceCount: int array
      DefenderId: string
      DefendingFieldId: string
      DefendingDiceCount: int array
      IsSuccessful: bool }

type Move = { Count: int; AddedFieldId: string }

type GameRules =
    { StartDiceCountPerField: int
      InitialDiceBuffer: int
      MaxDiceMovedPerTurn: int
      DiceGenerationMultiplier: float
      DeadPlayerMultiplier: float }

type GameState =
    { //NextGameId: string option
      //Players: Player array
      Fields: Fields
      //ActivePlayerId: string option
      //GameRound: int
      //PreviousAttack: Attack option
      //PreviousMove: Move option
      //Rules: GameRules 
      }

[<RequireQualifiedAccess>]
type Response =
| GameState of GameState

[<RequireQualifiedAccess>]
type Action =
| JoinGame
| AddBot of PlayerId: string * BotType: BotType 
| AddPlayer of PlayerId: string * PlayerName: string
| RemovePlayer of PlayerId: string
| MoveField of PlayerId: string * FromFieldId: string * ToFieldId: string
| EndRound of PlayerId: string
| Ready of PlayerId: string
| ReadyWithRules of PlayerId: string // TODO * GameRules: GameRules

let mutable selectedField: Field option = None

let mutable currentState =
    {
        NextGameId = None
        Players = [||]
        Fields = [||]
        ActivePlayerId = None
        GameRound = 0
        PreviousAttack = None
        PreviousMove = None
        Rules = {
            StartDiceCountPerField = 0
            InitialDiceBuffer = 0
            MaxDiceMovedPerTurn = 0
            DiceGenerationMultiplier = 0
            DeadPlayerMultiplier = 0
        }
    }
    // { Fields =
    //     [| { Id = 1
    //          Index = 1
    //          OwnerId = 0
    //          DiceCount = 1
    //          DiceAdded = 0
    //          Center = { X = 5; Y = 6 }
    //          Neighbors = [||]
    //          Coordinates =
    //            [| { X = 5; Y = 4 }
    //               { X = 5; Y = 5 }
    //               { X = 6; Y = 6 }
    //               { X = 7; Y = 5 }
    //               { X = 6; Y = 4 }
    //               { X = 5; Y = 7 }
    //               { X = 3; Y = 1 }
    //               { X = 3; Y = 2 }
    //               { X = 3; Y = 3 }
    //               { X = 2; Y = 4 }
    //               { X = 4; Y = 5 }
    //               { X = 3; Y = 6 }
    //               { X = 5; Y = 6 } |] }
    //        { Id = 2
    //          Index = 2
    //          OwnerId = 1
    //          DiceCount = 2
    //          DiceAdded = 0
    //          Center = { X = 12; Y = 11 }
    //          Neighbors = [||]
    //          Coordinates =
    //            [| { X = 12; Y = 12 }
    //               { X = 11; Y = 12 }
    //               { X = 10; Y = 12 }
    //               { X = 11; Y = 11 }
    //               { X = 12; Y = 11 }
    //               { X = 13; Y = 12 }
    //               { X = 14; Y = 12 }
    //               { X = 14; Y = 13 } |] }
    //        { Id = 3

    //          Index = 3
    //          OwnerId = 2
    //          DiceCount = 2
    //          DiceAdded = 0
    //          Center = { X = 9; Y = 5 }
    //          Neighbors = [||]
    //          Coordinates =
    //            [| { X = 7; Y = 6 }
    //               { X = 8; Y = 5 }
    //               { X = 8; Y = 6 }
    //               { X = 9; Y = 7 }
    //               { X = 9; Y = 5 }
    //               { X = 10; Y = 6 }
    //               { X = 10; Y = 7 }
    //               { X = 10; Y = 8 } |] } |] }
