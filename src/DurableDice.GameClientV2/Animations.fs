module Animations

open Browser
open Browser.Types
open Fable.Core.JsInterop
open Models
open Hexagon
open GameState

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
            ctx.lineWidth <- 0.4
            ctx.strokeStyle <- !^hexagons.Color
            ctx.globalAlpha <- 0.75
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
            ctx.globalAlpha <- 0.85
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
            ctx.globalAlpha <- 0.75
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
                ctx.strokeStyle <- !^"green"
                ctx.shadowBlur <- 0
                ctx.shadowColor <- null
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

type FailedCaptureAnimation (fieldId) = 
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
            gradient.addColorStop(0, hexagons.Color)
            gradient.addColorStop((progress / max), "red")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.4
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 0.75
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
                ctx.strokeStyle <- !^"red"
                ctx.shadowBlur <- 0
                ctx.shadowColor <- null
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

type GainedDiceAnimation (fieldId, diceAdded) = 
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
                float(9 - diceAdded) * 100.0)
            gradient.addColorStop(0, hexagons.Color)
            gradient.addColorStop((progress / max), "gold")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.4
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 0.75
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
                ctx.lineWidth <- float(diceAdded) * 0.2
                ctx.strokeStyle <- !^"white"
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
