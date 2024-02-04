open Browser
open Browser.Types
open Fable.Core.JsInterop

[<Measure>]
type px

[<Struct>]
type Coordinate = { X: int; Y: int }

type Coordinates = Coordinate array

[<Struct>]
type Position = { X: float<px>; Y: float<px> }

type Positions = Position array

type Edge = Position * Position
type Edges = Edge array

type EdgeType =
    | LeftTop
    | Left
    | LeftBottom
    | RightBottom
    | Right
    | RightTop

type Neighbors = (Coordinate * (EdgeType array)) array

type Field =
    { Id: int

      DiceCount: int
      DiceAdded: int

      Coordinates: Coordinate array
      Center: Coordinate }

type Fields = Field array

type GameState = { Fields: Fields }

let x = round (1000.0 / sqrt 3.0) / 1000.0
let r = 30.0<px>
let r2 = r * 2.0
let rx = r * x

let allEdges = [| LeftTop; Left; LeftBottom; RightBottom; Right; RightTop |]

let isIdentical (e1: Position) (e2: Position) =
    abs (e1.X - e2.X) < 0.1<px> && abs (e1.Y - e2.Y) < 0.1<px>

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

let toPosition (c: Coordinate) =
    { X = (float c.X) * r2 - ((float (c.Y % 2) * r))
      Y = (float c.Y) * (r2 - (r - rx)) }

let currentState =
    { Fields =
        [| { Id = 1
             DiceCount = 1
             DiceAdded = 0
             Center = { X = 7; Y = 5 }
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
             DiceCount = 2
             DiceAdded = 0
             Center = { X = 12; Y = 12 }
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
             DiceCount = 2
             DiceAdded = 0
             Center = { X = 12; Y = 12 }
             Coordinates =
               [| { X = 7; Y = 6 }
                  { X = 8; Y = 5 }
                  { X = 8; Y = 6 }
                  { X = 9; Y = 7 }
                  { X = 9; Y = 5 }
                  { X = 10; Y = 6 }
                  { X = 10; Y = 7 }
                  { X = 10; Y = 8 } |] } |] }

let neighbors (c: Coordinate) : (EdgeType * Coordinate) array =
    match c.Y % 2 with
    | 1 ->
        [| (Left, { X = c.X - 1; Y = c.Y })
           (LeftBottom, { X = c.X - 1; Y = c.Y + 1 })
           (RightBottom, { X = c.X; Y = c.Y + 1 })
           (Right, { X = c.X + 1; Y = c.Y })
           (RightTop, { X = c.X; Y = c.Y - 1 })
           (LeftTop, { X = c.X - 1; Y = c.Y - 1 }) |]
    | _ ->
        [| (LeftTop, { X = c.X; Y = c.Y - 1 })
           (Left, { X = c.X - 1; Y = c.Y })
           (LeftBottom, { X = c.X; Y = c.Y + 1 })
           (RightBottom, { X = c.X + 1; Y = c.Y + 1 })
           (Right, { X = c.X + 1; Y = c.Y })
           (RightTop, { X = c.X + 1; Y = c.Y - 1 }) |]

let calculateEdge (edge: EdgeType) =
    match edge with
    | LeftTop -> (fun c -> { X = c.X; Y = c.Y - r }, { X = c.X - r; Y = c.Y - rx })
    | Left -> (fun c -> { X = c.X - r; Y = c.Y - rx }, { X = c.X - r; Y = c.Y + rx })
    | LeftBottom -> (fun c -> { X = c.X - r; Y = c.Y + rx }, { X = c.X; Y = c.Y + r })

    | RightBottom -> (fun c -> { X = c.X; Y = c.Y + r }, { X = c.X + r; Y = c.Y + rx })
    | Right -> (fun c -> { X = c.X + r; Y = c.Y + rx }, { X = c.X + r; Y = c.Y - rx })
    | RightTop -> (fun c -> { X = c.X + r; Y = c.Y - rx }, { X = c.X; Y = c.Y - r })

let calculateEdges (p: Position) =
    allEdges |> Array.map (fun edge -> calculateEdge edge p)

let rec groupNeighbors (cs: Coordinates) : Neighbors array =
    if cs.Length = 0 then
        [||]
    else
        let rec loop (processedCoordinates: Coordinates) (nextCoordinates: Coordinates) =
            let coordinateNeighbors =
                nextCoordinates
                |> Array.map (fun coordinate ->
                    let neighborsOfCoordinate = coordinate |> neighbors

                    let externalEdges =
                        neighborsOfCoordinate
                        |> Array.filter (fun (_, c) -> cs |> Array.contains c = false)
                        |> Array.map fst

                    // for e in externalEdges do
                    //     printf "%d, %d has edge %s" coordinate.X coordinate.Y (e.ToString())

                    let unprocessedNeighborsOfCoordinate =
                        neighborsOfCoordinate
                        |> Array.filter (fun (_, c) ->
                            processedCoordinates |> Array.contains c = false
                            && cs |> Array.contains c = true)
                        |> Array.map snd

                    (coordinate, externalEdges), unprocessedNeighborsOfCoordinate)

            let coordinateEdges = coordinateNeighbors |> Array.map fst

            let nextNeighborsToProcess =
                coordinateNeighbors |> Array.map snd |> Array.collect id |> Array.distinct

            let processedCoordinates =
                processedCoordinates
                |> Array.append (coordinateEdges |> Array.map fst)
                |> Array.append nextNeighborsToProcess

            if nextNeighborsToProcess.Length = 0 then
                coordinateEdges
            else
                coordinateEdges
                |> Array.append (loop processedCoordinates nextNeighborsToProcess)

        let firstCoordinate = [| cs[0] |]

        let coordinatesWithEdges = loop firstCoordinate firstCoordinate

        let outGroup =
            cs
            |> Array.filter (fun c -> coordinatesWithEdges |> Array.map fst |> Array.contains c = false)

        if outGroup.Length = 0 then
            [| coordinatesWithEdges |]
        else
            [| coordinatesWithEdges |] |> Array.append (groupNeighbors outGroup)

let calculateOuterEdges (neighborGroups: Neighbors array) : Edges array =
    neighborGroups
    |> Array.map (fun group ->
        group
        |> Array.map (fun (c, edgeTypes) ->
            let p = c |> toPosition

            edgeTypes |> Array.map (fun edgeType -> calculateEdge edgeType p))

        |> Array.collect id)

let calculatePositions (neighborGroups: Neighbors array) : Positions array =
    neighborGroups
    |> Array.map (fun group -> group |> Array.map (fun (c, _) -> c |> toPosition))

let drawLine (ctx: CanvasRenderingContext2D) (color: string) ((p1, p2): Edge) =
    ctx.beginPath ()
    ctx.globalAlpha <- 1
    ctx.lineWidth <- 1
    ctx.strokeStyle <- !^color
    ctx.moveTo (float p1.X, float p1.Y)
    ctx.lineTo (float p2.X, float p2.Y)
    ctx.stroke ()
    ctx.closePath ()

let drawGlowingLine (ctx: CanvasRenderingContext2D) (color: string) ((p1, p2): Edge) =
    ctx.beginPath ()
    ctx.globalAlpha <- 1
    ctx.lineWidth <- 0
    ctx.shadowBlur <- 5
    ctx.shadowColor <- "white"
    ctx.moveTo (float p1.X, float p1.Y)
    ctx.lineTo (float p2.X, float p2.Y)
    ctx.stroke ()
    ctx.closePath ()

let drawAllEdges (ctx: CanvasRenderingContext2D) (color: string) (drawLine) (edges: Edges) =
    let drawLine = drawLine ctx color
    edges |> Array.iter drawLine

let drawHexagons (ctx: CanvasRenderingContext2D) (color: string) (positions: Positions) =
    positions
    |> Array.iter (fun position ->
        let edges = position |> calculateEdges

        let firstEdge = edges[0] |> fst

        ctx.beginPath ()
        ctx.fillStyle <- !^color
        ctx.lineWidth <- 0.2
        ctx.strokeStyle <- !^color
        ctx.globalAlpha <- 0.6

        ctx.moveTo (float firstEdge.X, float firstEdge.Y)

        for (_, edge) in edges do
            ctx.lineTo (float edge.X, float edge.Y)

        ctx.closePath ()
        ctx.stroke ()
        ctx.fill ())

let drawCoordinate (ctx: CanvasRenderingContext2D) (c: Coordinate) =
    ctx.globalAlpha <- 1

    ctx.strokeStyle <- !^ "white"
    ctx.fillStyle <- !^ "white"

    let pos = c |> toPosition

    ctx.fillText ($"{c.X},{c.Y}", float pos.X, float pos.Y)

let drawField (ctx: CanvasRenderingContext2D) (field: Field) =

    let color = color ()

    let drawHexagons = drawHexagons ctx color
    let drawEdges = drawAllEdges ctx color drawLine
    let drawOutline = drawAllEdges ctx color drawGlowingLine

    let neighborGroups = field.Coordinates |> groupNeighbors

    let neighborGroupPositions = neighborGroups |> calculatePositions
    let outerEdges = neighborGroups |> calculateOuterEdges

    neighborGroupPositions |> Array.iter drawHexagons
    outerEdges |> Array.iter drawEdges

    outerEdges |>Array.iter drawOutline

    // let drawCoordinate = drawCoordinate ctx
    // field.Coordinates |> Array.map drawCoordinate |> ignore

let mousemove (event: MouseEvent) = 

    printf "Mouse is at %f, %f" event.x event.y



    ()

let canvas = document.getElementById "canvas" :?> HTMLCanvasElement
let ctx = canvas.getContext_2d ()

//canvas.onmousemove <- mousemove

currentState.Fields |> Array.take 1 |> Array.map (drawField ctx) |> ignore


