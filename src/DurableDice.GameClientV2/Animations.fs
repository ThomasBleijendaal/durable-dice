module Animations

open Browser
open Browser.Types
open Fable.Core.JsInterop
open Models
open Hexagon
open GameState
open Player
open Theme

type IHexagonAnimation =
    abstract member Applies: Hexagon -> bool
    abstract member IsDone: bool
    abstract member AnimateHexagon: CanvasRenderingContext2D * string * FieldHexagons * Hexagon -> unit
    abstract member AnimateOuterEdge: CanvasRenderingContext2D * string * FieldHexagons * Hexagon -> unit

type DefaultAnimation () =
    interface IHexagonAnimation with
        member this.Applies (_) = true
        member this.IsDone = false
        member this.AnimateHexagon (ctx, color, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            ctx.beginPath ()

            match GameState.hoverField with
            | Some field when field.Id = hexagon.FieldId -> 
                let gradient = ctx.createRadialGradient(
                    float hexagons.CenterPosition.X, 
                    float hexagons.CenterPosition.Y, 
                    0.0,
                    float hexagons.CenterPosition.X, 
                    float hexagons.CenterPosition.Y,
                    300.0)
                gradient.addColorStop(1.0, "white")
                gradient.addColorStop(0, color)
                ctx.fillStyle <- !^gradient
            | _ -> ctx.fillStyle <- !^color
            
            ctx.lineWidth <- 1
            ctx.strokeStyle <- !^color
            ctx.globalAlpha <- 1
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, _, _, hexagon) =
            hexagon.OuterEdges |> Array.iter (fun (p1, p2) -> 
                ctx.beginPath ()
                ctx.globalAlpha <- 1
                ctx.lineWidth <- 1
                ctx.strokeStyle <- !^"black"
                ctx.shadowBlur <- 0
                ctx.shadowColor <- null
                ctx.moveTo (float p1.X, float p1.Y)
                ctx.lineTo (float p2.X, float p2.Y)
                ctx.stroke ()
                ctx.closePath ())

type SelectedAnimation () =
    interface IHexagonAnimation with
        member this.Applies (hexagon) = match GameState.selectedField with | Some field -> field.Id = hexagon.FieldId | None -> false
        member this.IsDone = false
        member this.AnimateHexagon (ctx, color, _, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            ctx.beginPath ()
            ctx.fillStyle <- !^color
            ctx.lineWidth <- 1
            ctx.strokeStyle <- !^color
            ctx.globalAlpha <- 1
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, _, _, hexagon) =
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

// TODO: animate from to
type CapturedAnimation (fieldId) = 
    let mutable progress = 0.0;
    let max = 100.0

    interface IHexagonAnimation with
        member this.Applies (hexagon) = hexagon.FieldId = fieldId
        member this.IsDone = 
            progress <- progress + 1.0
            progress > max
        member this.AnimateHexagon (ctx, color, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            let gradient = ctx.createRadialGradient(
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y, 
                0.0,
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y,
                100.0)
            gradient.addColorStop(1.0 - (progress / max), color)
            gradient.addColorStop(0, "lime")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.2
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 1
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, color, hexagons, hexagon) =
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

// TODO: animate failed from to
type FailedCaptureAnimation (fieldId) = 
    let mutable progress = 0.0;
    let max = 100.0

    interface IHexagonAnimation with
        member this.Applies (hexagon) = hexagon.FieldId = fieldId
        member this.IsDone = 
            progress <- progress + 1.0
            progress > max
        member this.AnimateHexagon (ctx, color, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            let gradient = ctx.createRadialGradient(
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y, 
                0.0,
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y,
                100.0)
            gradient.addColorStop(0, color)
            gradient.addColorStop((progress / max), "red")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.4
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 1
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, color, hexagons, hexagon) =
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
    let max = 100.0

    interface IHexagonAnimation with
        member this.Applies (hexagon) = hexagon.FieldId = fieldId
        member this.IsDone = 
            progress <- progress + 1.0
            progress > max
        member this.AnimateHexagon (ctx, color, hexagons, hexagon) =
            let edges = hexagon.AllEdges

            let firstEdge = edges[0] |> fst

            let gradient = ctx.createRadialGradient(
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y, 
                0.0,
                float hexagons.CenterPosition.X, 
                float hexagons.CenterPosition.Y,
                float(9 - diceAdded) * 10.0)
            gradient.addColorStop(0, color)
            gradient.addColorStop((progress / max), "gold")

            ctx.beginPath ()
            ctx.fillStyle <- !^gradient
            ctx.lineWidth <- 0.4
            ctx.strokeStyle <- !^gradient
            ctx.globalAlpha <- 1
            ctx.shadowBlur <- 0
            ctx.shadowColor <- null

            ctx.moveTo (float firstEdge.X, float firstEdge.Y)

            for (_, edge) in edges do
                ctx.lineTo (float edge.X, float edge.Y)

            ctx.closePath ()
            ctx.stroke ()
            ctx.fill ()

        member this.AnimateOuterEdge (ctx, color, hexagons, hexagon) =
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
    let apply (method) (ctx, color, hexagons, hexagon) : unit = 
        
        let runningAnimation = runningAnimations |> Array.tryFind (fun animation -> animation.Applies(hexagon))

        match runningAnimation with
        | Some animation -> method animation (ctx, color, hexagons, hexagon)
        | None when selectedAnimation.Applies (hexagon) -> method selectedAnimation (ctx, color, hexagons, hexagon)
        | None -> method defaultAnimation (ctx, color, hexagons, hexagon)
        
        if runningAnimation.IsSome then
            runningAnimations <- runningAnimations |> Array.filter (fun animation -> animation.IsDone = false)
