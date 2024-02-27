open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Microsoft.FSharp.Reflection
open Theme
open HexMath
open Models
open Field
open Player
open Hexagon
open GameState
open Animations

// TODO: fix
let endpoint = "http://localhost:7071"

let playerId =
    match System.Guid.TryParse(localStorage.getItem("player-id")) with
    | true, value -> value.ToString()
    | _ -> System.Guid.NewGuid().ToString()

localStorage.setItem("player-id", playerId)

let gameIdFromUrl = window.location.pathname.Replace("/", "")

let createGameId() = System.Guid.NewGuid().ToString().Substring(0, 6)

let gameId = 
    match (gameIdFromUrl, localStorage.getItem("game-id")) with
    | (url, storage) when url = storage && url.Length = 6 -> storage
    | (url, _) when url.Length = 6 -> url
    | _ -> createGameId()
    
if gameIdFromUrl <> gameId then
    history.pushState (None, document.title, $"{window.location.protocol}//{window.location.host}/{gameId}")

localStorage.setItem("game-id", gameId)

document.getElementById("shareurl").innerHTML <- window.location.href

let mutable currentFieldHexagons : FieldHexagons array = [||]

let mutable allHexagons : Hexagon array  = [||]

let mutable ownerColors : Map<string, string> = Map([||])

let canvas = document.getElementById "canvas" :?> HTMLCanvasElement

let mutable signalRCommand : string * obj -> unit = (fun _ -> ())

// TODO: handle promise
let sendAction (action: Action) = 
    match FSharpValue.GetUnionFields(action, typeof<Action>) with
    | union, data -> signalRCommand (union.Name, data[0])

let findHexagon (event: MouseEvent) =
    let rect = canvas.getBoundingClientRect ()
    let pos = { X = (event.x - rect.left) * 1.0<px>; Y = (event.y - rect.top) * 1.0<px> }
    let isHover = Hexagon.isInside pos
    allHexagons |> Array.tryFind isHover

let findField (id) = 
    GameState.currentState.Fields |> Array.tryFind (fun field -> field.Id = id)

let isActivePlayer () =
    match GameState.currentState.ActivePlayerId with 
    | Some id when id = playerId -> true 
    | _ -> false

let isAlivePlayer() =
    match GameState.currentState.Players |> Array.tryFind (Player.isPlayer playerId) with
    | Some player when player.ContinuousFieldCount = 0 -> false
    | _ -> true

let handleClickOnHexagon (foundHexagon: Hexagon option) =
    match GameState.currentRoundState, foundHexagon, GameState.selectedField with
    | RoundState.Idle, None, _ -> 
        GameState.selectedField <- None
        GameState.currentRoundState <- RoundState.Idle
    | RoundState.Idle, Some hexagon, _ -> 
        let foundField = findField hexagon.FieldId
        
        match foundField with
        | Some field when field.OwnerId = playerId ->
            GameState.selectedField <- findField hexagon.FieldId
            GameState.currentRoundState <- RoundState.FieldSelected
        | _ ->
            GameState.selectedField <- None
            GameState.currentRoundState <- RoundState.Idle
    
    | RoundState.FieldSelected, None, _ -> 
        GameState.selectedField <- None
        GameState.currentRoundState <- RoundState.Idle

    | RoundState.FieldSelected, Some hexagon, Some attackingField -> 
        let foundField = findField hexagon.FieldId

        match foundField with
        | None -> 
            GameState.selectedField <- None
            GameState.currentRoundState <- RoundState.Idle
        | Some field when field.OwnerId <> playerId && attackingField.Neighbors |> Array.contains field.Index -> 
            sendAction (Action.MoveField { FromFieldId = attackingField.Id; ToFieldId = field.Id })
            GameState.selectedField <- None
            GameState.currentRoundState <- RoundState.Idle
        | Some field when field.OwnerId = playerId -> 
            GameState.selectedField <- Some field
        | _ -> ()
    
    | _ -> ()

let handleMouseOnHexagon (foundHexagon: Hexagon option) =
    match GameState.currentRoundState, foundHexagon with
    | RoundState.Idle, Some hexagon -> 
        match findField hexagon.FieldId with 
        | Some field when field.OwnerId = playerId -> GameState.hoverField <- Some field
        | _ -> GameState.hoverField <- None

    | RoundState.FieldSelected, Some hexagon ->
        match findField hexagon.FieldId, GameState.selectedField with
        | Some field, Some selectedField when selectedField.Neighbors |> Array.contains field.Index -> GameState.hoverField <- Some field
        | _ -> GameState.hoverField <- None

    | _ -> GameState.hoverField <- None

// Interactions
let mouseclick (event: MouseEvent) =

    if isActivePlayer () then
        let foundHexagon = findHexagon event
        handleClickOnHexagon foundHexagon
    ()

let mousemove (event: MouseEvent) =

    if isActivePlayer () then
        let foundHexagon = findHexagon event
        handleMouseOnHexagon foundHexagon
    ()

canvas.onclick <- mouseclick
canvas.onmousemove <- mousemove

// SignalR signals

let updateGameState (state: GameState option) =
    match state with
    | None -> 
        GameState.currentUIState <- UIState.EnterName
    | Some state when state.Fields.Length = 0 -> 
        let player = state.Players |> Array.find (fun p -> p.Id = playerId)

        if player.IsReady = false then
            GameState.currentUIState <- UIState.EnterRules
        else
            GameState.currentUIState <- UIState.WaitForAllReady
        
        GameState.currentState <- state
    | Some state ->
        
        GameState.currentState <- state

        let nextUrl = document.getElementById("nexturl") :?> HTMLAnchorElement
        nextUrl.href <- $"{window.location.protocol}//{window.location.host}/{state.NextGameId}"

        if isActivePlayer () = false then
            GameState.resetRound ()
            GameState.currentUIState <- UIState.WatchingOtherPlayers
        else
            GameState.currentUIState <- UIState.PlayerTurn
        
        if GameState.currentState.Fields |> Array.groupBy (fun f -> f.OwnerId) |> Array.length = 1 then
            GameState.currentUIState <- UIState.MatchEnd

        if GameState.currentActionCount <> state.GameActionCount then
            
            GameState.currentActionCount <- state.GameActionCount

            match GameState.currentState.PreviousAttack with 
            | None -> ()
            | Some attack -> 
                let animation : IHexagonAnimation =
                    match attack.IsSuccessful with
                    | true -> CapturedAnimation(attack.DefendingFieldId)
                    | false -> FailedCaptureAnimation(attack.AttackingFieldId)

                runningAnimations <- runningAnimations |> Array.append [| animation |]

            if GameState.currentRound <> GameState.currentState.GameRound then
                GameState.currentRound <- GameState.currentState.GameRound

                runningAnimations <- runningAnimations 
                    |> Array.append (GameState.currentState.Fields 
                        |> Array.filter (fun field -> field.DiceAdded > 0)
                        |> Array.map (fun field -> GainedDiceAnimation(field.Id, field.DiceAdded)))

            if currentFieldHexagons.Length = 0 then
                currentFieldHexagons <- GameState.currentState.Fields |> Array.map (Field.groupHexagons GameState.currentState.Players)
                allHexagons <- 
                    currentFieldHexagons 
                    |> Array.map (fun hexGroups -> hexGroups.Hexagons |> Array.collect id)      
                    |> Array.collect id
                ownerColors <- Map(
                    GameState.currentState.Players
                    |> Array.map (fun player -> (player.Id, Theme.color (Some player.Index))))
        else
            console.log "Duplicate state received"

let registerCallback (callback: string * obj -> unit) =
    signalRCommand <- callback

// UI
let show (id) = document.getElementById(id).className <- "show";
let hide (id) = document.getElementById(id).className <- "hide";


let mutable previousTime = 0.0
let mutable previousUIState = UIState.Uninitialized

let rec draw (time: float) =
    let delta = time - previousTime

    if delta > 25.0 then
        previousTime <- time

        let w = canvas.width
        let h = canvas.height

        let ctx = canvas.getContext_2d ()

        ctx.clearRect (0, 0, w, h)

        let selectedFieldId =
            match GameState.selectedField with
            | Some field -> field.Id
            | None -> ""

        for group in currentFieldHexagons |> Seq.sortBy (fun g -> g.FieldId = selectedFieldId) do
            let apply (method) =
                for hexagons in group.Hexagons do
                    let field = GameState.currentState.Fields |> Array.find (fun field -> field.Id = group.FieldId)
                    let color = ownerColors.Item field.OwnerId

                    for hexagon in hexagons do
                        Animation.apply method (ctx, color, group, hexagon)

            apply (fun animation -> animation.AnimateHexagon)
            apply (fun animation -> animation.AnimateOuterEdge)

        GameState.drawPlayers ctx GameState.currentState |> ignore
        GameState.drawDice ctx GameState.currentState |> ignore
        GameState.drawTurn ctx GameState.currentState |> ignore
        
        if previousUIState <> GameState.currentUIState then
        
            match GameState.currentUIState with
            | UIState.EnterName -> 
                hide("ready")
                hide("end")
                show("form")
                show("joinGame")
                hide("rules")
                hide("gameRules")
                hide("dead")
                hide("winner")
            | UIState.EnterRules ->
                show("ready")
                hide("end")
                show("form")
                hide("joinGame")
                show("rules")
                show("gameRules")
                hide("dead")
                hide("winner")
            | UIState.WaitForAllReady ->
                hide("ready")
                hide("end")
                hide("form")
                hide("joinGame")
                hide("rules")
                hide("gameRules")
                hide("dead")
                hide("winner")
            | UIState.WatchingOtherPlayers ->
                hide("ready")
                hide("end")
                hide("form")
                hide("joinGame")
                hide("rules")
                hide("gameRules")
                if isAlivePlayer() then
                    hide("dead")
                else
                    show("dead")
                hide("winner")
            | UIState.PlayerTurn ->
                hide("ready")
                show("end")
                hide("form")
                hide("joinGame")
                hide("rules")
                hide("gameRules")
                hide("dead")
                hide("winner")
            | UIState.MatchEnd ->
                hide("ready")
                hide("end")
                hide("form")
                hide("joinGame")
                hide("rules")
                hide("gameRules")
                if isAlivePlayer() then
                    hide("dead")
                    show("winner")
                else
                    show("dead")
                    hide("winner")

            | _ -> ()


            previousUIState <- GameState.currentUIState

            ()

    window.requestAnimationFrame (draw >> ignore)

draw 0 |> ignore

updateGameState None 
