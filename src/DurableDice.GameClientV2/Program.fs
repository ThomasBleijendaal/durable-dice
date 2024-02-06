open Browser
open Browser.Types
open Fable.Core.JsInterop
open Fable.SignalR
open Theme
open HexMath
open Models
open Field
open Hexagon
open GameState
open Animations

let endpoint = "http://localhost:7071"

let gameId = System.Guid.NewGuid().ToString()
let playerId = System.Guid.NewGuid().ToString()

let mutable currentFieldHexagons : FieldHexagons array = [||]

let mutable allHexagons : Hexagon array  = [||]
    

let canvas = document.getElementById "canvas" :?> HTMLCanvasElement

let selectField (foundHexagon: Hexagon option) =
    let findField (id) =
        currentState.Fields |> Array.tryFind (fun field -> field.Id = id)

    match (foundHexagon, selectedField) with
    | Some hex, None -> selectedField <- findField hex.FieldId
    | Some hex, Some field when hex.FieldId <> field.Id -> selectedField <- findField hex.FieldId
    | _ -> selectedField <- None

// SignalR

let headers = Map([| "x-playerid", playerId; "x-game", gameId |])

let signalRHub =
    SignalR.connect<Action, unit, unit, GameState, unit> (fun hub ->
        hub
            .withUrl(
                endpoint,
                (fun options -> options.header(headers))
            )
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Debug)
            .onMessage (fun response -> 
                printf "PAAAARSE"
                //match response with
                //| Response.GameState state -> currentState <- state
                
                // if currentFieldHexagons.Length = 0 then

                //     currentFieldHexagons <- currentState.Fields |> Array.map Field.groupHexagons
                //     allHexagons <- 
                //         currentFieldHexagons 
                //         |> Array.map (fun hexGroups -> hexGroups.Hexagons |> Array.collect id)      
                //         |> Array.collect id

                ))

signalRHub.startNow ()
signalRHub.sendNow Action.JoinGame


// Interactions
let mouseclick (event: MouseEvent) =

    let rect = canvas.getBoundingClientRect ()

    let pos =
        { X = (event.x - rect.left) * 1.0<px>
          Y = (event.y - rect.top) * 1.0<px> }

    let isHover = Hexagon.isInside pos

    let foundHexagon = allHexagons |> Array.tryFind isHover

    selectField foundHexagon


    promise {
        do! signalRHub.sendAsPromise (Action.AddPlayer (playerId, "Test"))
        do! signalRHub.sendAsPromise (Action.AddBot (playerId, BotType.NerdBot))
        do! signalRHub.sendAsPromise (Action.AddBot (playerId, BotType.NerdBot))
        do! signalRHub.sendAsPromise (Action.ReadyWithRules (playerId))
    } |> ignore

// match foundHexagon with
// | Some hexagon -> runningAnimations <- [| GainedDiceAnimation(hexagon.FieldId, 1) |]
// | None -> ()

canvas.onclick <- mouseclick

// UI
let mutable previousTime = 0.0

let rec draw (time: float) =
    let delta = time - previousTime

    if delta > 10.0 then
        previousTime <- time

        let w = canvas.width
        let h = canvas.height

        let ctx = canvas.getContext_2d ()

        ctx.clearRect (0, 0, w, h)

        let selectedFieldId =
            match selectedField with
            | Some field -> field.Id
            | None -> -1

        for group in currentFieldHexagons |> Seq.sortBy (fun g -> g.FieldId = selectedFieldId) do
            let apply (method) =
                for hexagons in group.Hexagons do
                    for hexagon in hexagons do
                        Animation.apply method (ctx, group, hexagon)

            apply (fun animation -> animation.AnimateHexagon)
            apply (fun animation -> animation.AnimateOuterEdge)

    window.requestAnimationFrame (draw >> ignore)

draw 0 |> ignore
