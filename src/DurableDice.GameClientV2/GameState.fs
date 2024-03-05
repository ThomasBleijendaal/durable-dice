module GameState

open Field
open Player
open Browser.Types
open Fable.Core.JsInterop
open HexMath
open Field
open Models

open Browser

type GameState =
    { NextGameId: string option
      Players: Player array
      Fields: Fields
      ActivePlayerId: string option
      GameRound: int
      GameActionCount: int64
      PreviousAttack: Attack option
      PreviousMove: Move option
      Rules: GameRules }

[<RequireQualifiedAccess>]
type Response = GameState of GameState

[<RequireQualifiedAccess>]
type UIState =
| Uninitialized
| EnterName
| EnterRules
| WaitForAllReady
| WatchingOtherPlayers
| PlayerTurn
| MatchEnd
| Statistics

[<RequireQualifiedAccess>]
type RoundState =
| Idle
| FieldSelected

module GameState =
    let mutable currentUIState = UIState.Uninitialized

    let mutable currentRound = 0
    let mutable currentActionCount = -2L

    let mutable selectedField: Field option = None
    let mutable hoverField: Field option = None
    let mutable targetField: Field option = None

    let mutable currentRoundState = RoundState.Idle

    let mutable currentState =
        { NextGameId = None
          Players = [||]
          Fields = [||]
          ActivePlayerId = None
          GameRound = 0
          GameActionCount = -1
          PreviousAttack = None
          PreviousMove = None
          Rules =
            { StartDiceCountPerField = 0
              InitialDiceBuffer = 0
              MaxDiceMovedPerTurn = 0
              DiceGenerationMultiplier = 0
              DeadPlayerMultiplier = 0 }
        }


    let resetRound () = 
        selectedField <- None
        hoverField <- None
        currentRoundState <- RoundState.Idle

    let drawPlayers (ctx: CanvasRenderingContext2D) (state: GameState) =
        let boxWidth = 120.0<px>

        let playerCount = state.Players |> Array.length

        let totalWidth = float (playerCount) * boxWidth
        let offset = (HexMath.width - totalWidth) / 2.0

        let mutable i = 0

        for player in state.Players do
            let x = offset + (float(i) * boxWidth)

            let isDead = player.ContinuousFieldCount = 0

            let isActive =
                match state.ActivePlayerId with
                | Some id when id = player.Id -> true
                | _ -> false

            ctx.beginPath ()

            if isDead then
                ctx.fillStyle <- !^ "silver"
                ctx.strokeStyle <- !^ "silver"
            else
                ctx.fillStyle <- !^(Player.color player)
                ctx.strokeStyle <- !^(Player.color player)

            if isActive && not isDead then
                ctx.globalAlpha <- 1
                ctx.lineWidth <- 2
                ctx.strokeStyle <- !^ "white"
            else
                ctx.lineWidth <- 0.4
                ctx.globalAlpha <- 0.75

            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float x, 10.0)
            ctx.lineTo (float x, 30.0)
            ctx.lineTo (float (x + boxWidth - 3.0<px>), 30.0)
            ctx.lineTo (float (x + boxWidth - 3.0<px>), 10.0)
            ctx.lineTo (float x, 10.0)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

            if isActive && not isDead then
                ctx.fillStyle <- !^ "white"
            else
                ctx.fillStyle <- !^ "black"

            ctx.font <- "12px Verdana"
            ctx.textAlign <- "center"
            ctx.fillText ($"{player.Name} ({player.ContinuousFieldCount})", float (x + 3.0<px> + (boxWidth / 2.0)), 24.0, float boxWidth)

            i <- i + 1

        ignore

    let drawDice (ctx: CanvasRenderingContext2D) (state: GameState) =

        for field in state.Fields do
            
            let position = field.Center |> Coordinate.toPosition

            ctx.fillStyle <- !^ "white"
            ctx.font <- "12px Verdana"
            ctx.textAlign <- "center"
            ctx.fillText ($"{field.DiceCount}", float (position.X), float (position.Y) + 4.0, 30.0)

        ignore

    let drawTurn (ctx: CanvasRenderingContext2D) (state: GameState) =
        
        match state.PreviousAttack, state.PreviousMove with
        | Some attack, None -> 

            let attackingPlayer = state.Players |> Array.find (Player.isPlayer attack.AttackerId)
            let defendingPlayer = state.Players |> Array.find (Player.isPlayer attack.DefenderId)
            let attackingColor = Player.color attackingPlayer
            let defendingColor = Player.color defendingPlayer

            let diceDifference = (attack.AttackingDiceCount |> Array.sum) - (attack.DefendingDiceCount |> Array.sum)

            let attackComment =
                match diceDifference with
                | i when i < -15 -> "got owned by"
                | i when i < -10 -> "never stood a change against"
                | i when i < -5 -> "underestimated"
                | i when i < 0 -> "came short against"
                | 0 -> "did not have enough for"
                | i when i < 5 -> "barely won of"
                | i when i < 10 -> "bullied"
                | i when i < 15 -> "destroyed"
                | _ -> "obliterated"

            ctx.fillStyle <- !^ "black"
            ctx.font <- "12px Verdana"
            ctx.textAlign <- "center"
            ctx.fillText (attackComment, 615, 1020, 220)


            let dicePositions = [| 0,0; 1,0; 2,0; 3,0; 0,1; 1,1; 2,1; 3,1 |]
            let pipPositions = [| 
                0,0,0, 0,0,0, 0,0,0;
                0,0,0, 0,1,0, 0,0,0;
                1,0,0, 0,0,0, 0,0,1;
                1,0,0, 0,1,0, 0,0,1;
                1,0,1, 0,0,0, 1,0,1;
                1,0,1, 0,1,0, 1,0,1;
                1,0,1, 1,0,1, 1,0,1;
                |]

            let attackPosition : Position = { X = 480.0<px>; Y = 1000.0<px> }
            let defendPosition : Position = { X = 720.0<px>; Y = 1000.0<px> }

            let dieSize = 30.0<px>

            let drawDie (color: string) (pos: Position) =
                ctx.beginPath ()

                ctx.fillStyle <- !^color
                ctx.strokeStyle <- !^"black"
                ctx.lineWidth <- 2

                let x = pos.X
                let y = pos.Y
                
                ctx.moveTo (float x, float y)
                ctx.lineTo (float x, float y)
                ctx.lineTo (float (x + dieSize), float y )
                ctx.lineTo (float (x + dieSize), float (y + dieSize))
                ctx.lineTo (float x, float (y + dieSize))

                ctx.closePath ()
                ctx.stroke ()
                ctx.fill ()
                
            let drawAttackDie = drawDie attackingColor
            let drawDefendDie = drawDie defendingColor

            let drawPip (delta: Position) (pos: Position)  =
                ctx.beginPath ()

                ctx.fillStyle <- !^"black"
                ctx.strokeStyle <- !^"black"
                ctx.lineWidth <- 2

                let x = float(pos.X + delta.X)
                let y = float(pos.Y + delta.Y)
                
                ctx.moveTo (x, y)
                ctx.arc (x, y, 2.0, 0.0, System.Math.PI * 2.0)
                
                ctx.closePath ()
                ctx.stroke ()
                ctx.fill ()
                
            let drawTlPip = drawPip ({ X = 5.0<px>; Y = 5.0<px> })
            let drawTrPip = drawPip ({ X = 25.0<px>; Y = 5.0<px> })
            let drawLPip = drawPip ({ X = 5.0<px>; Y = 25.0<px> })
            let drawCPip = drawPip ({ X = 15.0<px>; Y = 15.0<px> })
            let drawRPip = drawPip ({ X = 25.0<px>; Y = 25.0<px> })
            let drawBlPip = drawPip ({ X = 5.0<px>; Y = 25.0<px> })
            let drawBrPip = drawPip ({ X = 25.0<px>; Y = 25.0<px> })

            let tl (x, _, _, _, _, _, _, _, _) = x
            let tr (_, _, x, _, _, _, _, _, _) = x
            let l (_, _, _, x, _, _, _, _, _) = x
            let c (_, _, _, _, x, _, _, _, _) = x
            let r (_, _, _, _, _, x, _, _, _) = x
            let bl (_, _, _, _, _, _, x, _, _) = x
            let br (_, _, _, _, _, _, _, _, x) = x

            let drawPips (pos: Position) (pips) =
                if tl pips = 1 then drawTlPip pos
                if tr pips = 1 then drawTrPip pos
                if l pips = 1 then drawLPip pos
                if c pips = 1 then drawCPip pos
                if r pips = 1 then drawRPip pos
                if bl pips = 1 then drawBlPip pos
                if br pips = 1 then drawBrPip pos
            
            attack.AttackingDiceCount
            |> Array.iteri (fun index count ->
                let position = dicePositions[index]
                let pos = { 
                    X = attackPosition.X - (float(position |> fst) * 40.0<px>);
                    Y = attackPosition.Y + (float(position |> snd) * 40.0<px>) }
                drawAttackDie (pos)
                drawPips (pos) (pipPositions[count])
                )
            attack.DefendingDiceCount
            |> Array.iteri (fun index count ->
                let position = dicePositions[index]
                let pos = { 
                    X = defendPosition.X + (float(position |> fst) * 40.0<px>);
                    Y = defendPosition.Y + (float(position |> snd) * 40.0<px>) }
                drawDefendDie (pos)
                drawPips (pos) (pipPositions[count])
                
                )

            ()
        | None, Some move ->

            // TODO

            ()
        | _ -> ()
            

        ignore