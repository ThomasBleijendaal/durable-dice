open Browser
open Browser.Types
open Fable.Core.JsInterop
open Theme
open HexMath
open Models
open Field
open Hexagon

// mutable game state via signal r
type GameState = { Fields: Fields }

// mutable UI state
let mutable selectedField : Field option = None

type IHexagonAnimation =
    abstract member Applies: Hexagon -> bool
    abstract member IsDone: bool
    abstract member AnimateHexagon: CanvasRenderingContext2D * FieldHexagons * Hexagon -> unit
    abstract member AnimateOuterEdge: CanvasRenderingContext2D * FieldHexagons * Hexagon -> unit

type DefaultAnimation () =
    interface IHexagonAnimation with
        member this.Applies (_) = true
        member this.IsDone = false
        member this.AnimateHexagon (ctx, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            ctx.beginPath ()
            ctx.fillStyle <- !^hexagons.Color
            ctx.lineWidth <- 0.2
            ctx.strokeStyle <- !^hexagons.Color
            ctx.globalAlpha <- 0.6
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, hexagons, hexagon) =
            hexagon.OuterEdges |> Array.iter (fun (p1, p2) -> 
                ctx.beginPath ()
                ctx.globalAlpha <- 1
                ctx.lineWidth <- 1
                ctx.strokeStyle <- !^hexagons.Color
                ctx.shadowBlur <- 0
                ctx.shadowColor <- null
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

type SelectedAnimation () =
    interface IHexagonAnimation with
        member this.Applies (hexagon) = match selectedField with | Some field -> field.Id = hexagon.FieldId | None -> false
        member this.IsDone = false
        member this.AnimateHexagon (ctx, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            ctx.beginPath ()
            ctx.fillStyle <- !^hexagons.Color
            ctx.lineWidth <- 1
            ctx.strokeStyle <- !^hexagons.Color
            ctx.globalAlpha <- 0.8
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, _, hexagon) =
            hexagon.OuterEdges |> Array.iter (fun (p1, p2) -> 
                ctx.beginPath ()
                ctx.globalAlpha <- 1
                ctx.lineWidth <- 2
                ctx.strokeStyle <- !^"white"
                ctx.shadowBlur <- 5
                ctx.shadowColor <- "white"
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

type CapturedAnimation (fieldId) = 
    let mutable progress = 0.0;
    let max = 1000.0

    interface IHexagonAnimation with
        member this.Applies (hexagon) = hexagon.FieldId = fieldId
        member this.IsDone = 
            progress <- progress + 1.0
            progress > max
        member this.AnimateHexagon (ctx, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            let gradient = ctx.createRadialGradient(
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y, 
                0.0,
                float hexagon.Position.X, 
                float hexagon.Position.Y,
                500.0)
            gradient.addColorStop(1.0 - (progress / max), hexagons.Color)
            gradient.addColorStop(0, "lime")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.2
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 0.6
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, hexagons, hexagon) =
            hexagon.OuterEdges |> Array.iter (fun (p1, p2) -> 
                ctx.beginPath ()
                ctx.globalAlpha <- 1
                ctx.lineWidth <- 1
                ctx.strokeStyle <- !^hexagons.Color
                ctx.shadowBlur <- 0
                ctx.shadowColor <- null
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

let defaultAnimation : IHexagonAnimation = DefaultAnimation()
let selectedAnimation : IHexagonAnimation = SelectedAnimation()

let mutable runningAnimations: IHexagonAnimation array = [||]

module Animation = 
    let apply (method) (ctx, color, hexagon) = 
        
        let runningAnimation = runningAnimations |> Array.tryFind (fun animation -> animation.Applies(hexagon))

        match runningAnimation with
        | Some animation -> method animation (ctx, color, hexagon)
        | None when selectedAnimation.Applies (hexagon) -> method selectedAnimation (ctx, color, hexagon)
        | None -> method defaultAnimation (ctx, color, hexagon)
        
        if runningAnimation.IsSome then
            runningAnimations <- runningAnimations |> Array.filter (fun animation -> animation.IsDone = false)

let mutable capturedField : Field option = None

let currentState =
    { Fields =
        [| { Id = 1
             PlayerId = 0
             DiceCount = 1
             DiceAdded = 0
             Center = { X = 5; Y = 6 }
             Coordinates =
               [| { X = 5; Y = 4 }
                  { X = 5; Y = 5 }
                  { X = 6; Y = 6 }
                  { X = 7; Y = 5 }
                  { X = 6; Y = 4 }
                  { X = 5; Y = 7 }
                  { X = 3; Y = 1 }
                  { X = 3; Y = 2 }
                  { X = 3; Y = 3 }
                  { X = 2; Y = 4 }
                  { X = 4; Y = 5 }
                  { X = 3; Y = 6 }
                  { X = 5; Y = 6 } |] }
           { Id = 2
             PlayerId = 1
             DiceCount = 2
             DiceAdded = 0
             Center = { X = 12; Y = 11 }
             Coordinates =
               [| { X = 12; Y = 12 }
                  { X = 11; Y = 12 }
                  { X = 10; Y = 12 }
                  { X = 11; Y = 11 }
                  { X = 12; Y = 11 }
                  { X = 13; Y = 12 }
                  { X = 14; Y = 12 }
                  { X = 14; Y = 13 } |] }
           { Id = 3
             PlayerId = 2
             DiceCount = 2
             DiceAdded = 0
             Center = { X = 9; Y = 5 }
             Coordinates =
               [| { X = 7; Y = 6 }
                  { X = 8; Y = 5 }
                  { X = 8; Y = 6 }
                  { X = 9; Y = 7 }
                  { X = 9; Y = 5 }
                  { X = 10; Y = 6 }
                  { X = 10; Y = 7 }
                  { X = 10; Y = 8 } |] } |] }

let drawFieldHexagons (ctx: CanvasRenderingContext2D) (hexagons: FieldHexagons) =
    
    for group in hexagons.Hexagons do
        for hexagon in group do
            Animation.apply (fun animation -> animation.AnimateHexagon) (ctx, hexagons, hexagon)

    for group in hexagons.Hexagons do
        for hexagon in group do
            Animation.apply (fun animation -> animation.AnimateOuterEdge) (ctx, hexagons, hexagon)

let currentFieldHexagons = currentState.Fields |> Array.map Field.groupHexagons

capturedField <- Some (currentState.Fields[0])

let allHexagons = currentFieldHexagons |> Array.map (fun hexGroups -> hexGroups.Hexagons |> Array.collect id) |> Array.collect id

let canvas = document.getElementById "canvas" :?> HTMLCanvasElement

let selectField (foundHexagon: Hexagon option) = 
    let findField (id) = currentState.Fields |> Array.tryFind (fun field -> field.Id = id)

    match (foundHexagon, selectedField) with
    | Some hex, None -> selectedField <- findField hex.FieldId
    | Some hex, Some field when hex.FieldId <> field.Id -> selectedField <- findField hex.FieldId
    | _ -> selectedField <- None

let mouseclick (event: MouseEvent) =

    let rect = canvas.getBoundingClientRect ()

    let pos =
        { X = (event.x - rect.left) * 1.0<px>
          Y = (event.y - rect.top) * 1.0<px> }

    let isHover = Hexagon.isInside pos

    let foundHexagon = allHexagons |> Array.tryFind isHover

    // selectField foundHexagon

    match foundHexagon with
    | Some hexagon -> runningAnimations <- [| CapturedAnimation(hexagon.FieldId) |]
    | None -> ()

canvas.onclick <- mouseclick

let mutable previousTime = 0.0

let rec draw (time: float) =
    let delta = time - previousTime
    
    if delta > 10.0 then
        previousTime <- time
        
        let w = canvas.width
        let h = canvas.height

        let ctx = canvas.getContext_2d ()

        ctx.clearRect (0, 0, w, h)

        for group in currentFieldHexagons do
            drawFieldHexagons ctx group

    window.requestAnimationFrame(draw >> ignore)

draw 0 |> ignore
