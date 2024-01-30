open Browser
open Browser.Types
open Fable.Core.JsInterop

[<Measure>]
type px

[<Struct>]
type Coordinate = { X: float; Y: float }

[<Struct>]
type Position = { X: float<px>; Y: float<px> }

type Edge = Position * Position
type Edges = Edge array

type EdgeType =
    | LeftTop
    | Left
    | LeftBottom
    | RightBottom
    | Right
    | RightTop

type Field =
    { Id: int

      DiceCount: int
      DiceAdded: int

      Coordinates: Coordinate array
      Center: Coordinate }

type GameState = { Fields: Field array }


let x = 1.0 / sqrt 3.0
let r = 20.0<px>
let r2 = r * 2.0
let rx = r * x

let allEdges = [| LeftTop; Left; LeftBottom; RightBottom; Right; RightTop |]

let toPixelCoordinate (c: Coordinate) =
    { X = c.X * r2 - ((c.Y % 2.0 * r))
      Y = c.Y * (r2 - (r - rx)) }

let currentState =
    { Fields =
        [| { Id = 1
             DiceCount = 1
             DiceAdded = 0
             Center = { X = 10; Y = 10 }
             Coordinates =
               [| { X = 5; Y = 5 }
                  { X = 5; Y = 6 }
                  { X = 7; Y = 5 }
                  { X = 4; Y = 5 }
                  { X = 4; Y = 6 }
                  { X = 2; Y = 5 }
                  { X = 6; Y = 6 }
                  { X = 8; Y = 8 } |] } |] }

//let calculateOutline (c: Coordinate array) =

let neighbors (c: Coordinate) : (EdgeType * Coordinate) array =
    match c.Y % 2.0 with
    | 1.0 ->
        [| (Left, { X = c.X - 1.0; Y = c.Y })
           (LeftBottom, { X = c.X - 1.0; Y = c.Y + 1.0 })
           (RightBottom, { X = c.X; Y = c.Y + 1.0 })
           (Right, { X = c.X + 1.0; Y = c.Y })
           (RightTop, { X = c.X; Y = c.Y - 1.0 })
           (LeftTop, { X = c.X - 1.0; Y = c.Y - 1.0 }) |]
    | _ ->
        [| (LeftTop, { X = c.X; Y = c.Y - 1.0 })
           (Left, { X = c.X - 1.0; Y = c.Y })
           (LeftBottom, { X = c.X; Y = c.Y + 1.0 })
           (RightBottom, { X = c.X + 1.0; Y = c.Y + 1.0 })
           (Right, { X = c.X + 1.0; Y = c.Y })
           (RightTop, { X = c.X + 1.0; Y = c.Y - 1.0 }) |]

let calculateEdge (edge: EdgeType) =
    match edge with
    | LeftTop -> (fun c -> { X = c.X; Y = c.Y - r }, { X = c.X - r; Y = c.Y - rx })
    | Left -> (fun c -> { X = c.X - r; Y = c.Y - rx }, { X = c.X - r; Y = c.Y + rx })
    | LeftBottom -> (fun c -> { X = c.X - r; Y = c.Y + rx }, { X = c.X; Y = c.Y + r })

    | RightBottom -> (fun c -> { X = c.X; Y = c.Y + r }, { X = c.X + r; Y = c.Y + rx })
    | Right -> (fun c -> { X = c.X + r; Y = c.Y + rx }, { X = c.X + r; Y = c.Y - rx })
    | RightTop -> (fun c -> { X = c.X + r; Y = c.Y - rx }, { X = c.X; Y = c.Y - r })

let calculateEdges (p: Position) =
    allEdges 
    |> Array.map (fun edge -> calculateEdge edge p)

//let combineEdges (edges: Edge array) =

let calculateOuterEdges (cs: Coordinate array) : Edges array =

    cs
    |> Array.map (fun c -> 
        let p = c |> toPixelCoordinate
        let neighbors = c |> neighbors
        
        neighbors
        |> Array.map (fun neighbor ->
            if cs |> Array.contains (snd neighbor) then
                None
            else
                Some (calculateEdge (fst neighbor) p))
        |> Array.choose id)
    


let color () =
    match System.Convert.ToInt32((new System.Random()).NextDouble() * 8.0) with
    | 1 -> "#ffc83d"
    | 2 -> "#e3008c"
    | 3 -> "#4f6bed"
    | 4 -> "#ca5010"
    | 5 -> "#00b294"
    | 6 -> "#498205"
    | 7 -> "#881798"
    | _ -> "#986f0b"

let drawEdges (ctx: CanvasRenderingContext2D) (color: string) (edges: Edges) =
    if edges.Length > 0 then

        let firstEdge = edges[0]

        let rec loop (currentEdge: Edge) =
            let startPos = currentEdge |> fst
            let endPos = currentEdge |> snd

            let nextEdge = edges |> Array.tryFind (fun o -> o |> fst = endPos)

            ctx.lineTo (float startPos.X, float startPos.Y)
            ctx.lineTo (float endPos.X, float endPos.Y)

            match nextEdge with
            | Some edge when firstEdge <> edge -> loop edge
            | _ -> ()

        ctx.beginPath ()
        ctx.globalAlpha <- 0.6
        ctx.fillStyle <- !^color

        let first = firstEdge |> fst
        ctx.moveTo (float first.X, float first.Y)
        
        loop firstEdge

        ctx.fill ()
        ctx.closePath ()




let drawEdgesOld (ctx: CanvasRenderingContext2D) (color: string) (edges: Edges) =
    if edges.Length > 0 then
        ctx.beginPath ()
        ctx.globalAlpha <- 0.6
        ctx.fillStyle <- !^color

        let first = edges[0] |> fst
        ctx.moveTo (float first.X, float first.Y)

        for edge in edges |> Array.map fst do
            ctx.lineTo (float edge.X, float edge.Y)

        ctx.fill ()
        ctx.closePath ()

        ctx.beginPath ()
        ctx.globalAlpha <- 1

        ctx.strokeStyle <- !^color
        ctx.lineWidth <- 1

        ctx.moveTo (float first.X, float first.Y)

        for edge in edges |> Array.map fst do
            ctx.lineTo (float edge.X, float edge.Y)

        ctx.lineTo (float first.X, float first.Y)

        ctx.stroke ()
        ctx.closePath ()

let drawField (ctx: CanvasRenderingContext2D) (field: Field) =

    let draw = drawEdges ctx (color ())
    let drawOld = drawEdgesOld ctx (color ())

    field.Coordinates
    |> Array.map (toPixelCoordinate >> calculateEdges)
    |> Array.map drawOld
    |> ignore

    field.Coordinates 
    |> calculateOuterEdges
    |> Array.map draw
    |> ignore

    //let draw2 = drawEdges ctx (color ())
    // field.Coordinates
    // |> Array.map neighbors
    // |> Array.collect id
    // |> Array.map (snd >> toPixelCoordinate >> calculateEdges)
    // |> Array.map draw2
    // |> ignore

    ignore


let canvas = document.getElementById "canvas" :?> HTMLCanvasElement
let ctx = canvas.getContext_2d ()



currentState.Fields |> Array.map (drawField ctx) |> ignore
