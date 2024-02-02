open Browser
open Browser.Types
open Fable.Core.JsInterop

[<Measure>] type px

[<Struct>]
type Coordinate = { X: float; Y: float }

type Coordinates = Coordinate array

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

let toPixelCoordinate (c: Coordinate) =
    { X = c.X * r2 - ((c.Y % 2.0 * r))
      Y = c.Y * (r2 - (r - rx)) }

let currentState =
    { Fields =
        [| { Id = 1
             DiceCount = 1
             DiceAdded = 0
             Center = { X = 7; Y = 5 }
             Coordinates =
               [| { X = 5; Y = 5 }
                //   { X = 8; Y = 8 }
                //   { X = 7; Y = 5 }
                //   { X = 4; Y = 5 }
                //   { X = 4; Y = 6 }
                //   { X = 2; Y = 5 }
                //   { X = 6; Y = 6 }
                  { X = 5; Y = 4 } |] }
        //    { Id = 2
        //      DiceCount = 2
        //      DiceAdded = 0
        //      Center = { X = 12; Y = 12 }
        //      Coordinates =
        //        [| { X = 12; Y = 12 }
        //           { X = 11; Y = 12 }
        //           { X = 10; Y = 12 }
        //           { X = 11; Y = 11 }
        //           { X = 12; Y = 11 }
        //           { X = 13; Y = 12 }
        //           { X = 14; Y = 12 }
        //           { X = 14; Y = 13 } |] } 
                  |] }

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
    allEdges |> Array.map (fun edge -> calculateEdge edge p)

// TODO: output Neighbors array
let rec groupCoordinates (cs: Coordinates) : Neighbors array =
    if cs.Length = 0 then
        [||]
    else
        let firstCoordinate = [| (cs[0], cs[0] |> neighbors |> Array.map fst) |]

        
        for (c, es) in firstCoordinate do 
            printf "Coordinate %f, %f" c.X c.Y
            for e in es do
                printf "Edge %s" (e.ToString())

        printf "-------"

        let rec loop (group: Neighbors) =
            let groupCoordinates =
                group
                |> Array.map fst

            let groupNeighbors =
                group
                //|> Array.map (fun c -> c |> neighbors |> Array.map snd)
                |> Array.map (fun c -> 
                    c 
                    |> fst 
                    |> neighbors
                    // TODO: this neighbors EDGETYPE must be put on GROUP while the COORDINATES are the new things to check
                    |> Array.groupBy snd
                    |> Array.map (fun (c, g) -> (c, g |> Array.map fst)))
                //|> Array.groupBy snd
                |> Array.collect id
                |> Array.filter (fun (c, edgeTypes) -> 
                    groupCoordinates |> Array.contains c = false && cs |> Array.contains c = true)

            for (c, es) in groupNeighbors do 
                printf "Coordinate %f, %f" c.X c.Y
                for e in es do
                    printf "Edge %s" (e.ToString())

            if groupNeighbors.Length = 0 then
                group
            else
                loop (group |> Array.append groupNeighbors)

        let group = loop firstCoordinate

        let groupC = group |> Array.map fst

        let outGroup = cs |> Array.filter (fun c -> groupC |> Array.contains c = false)

        if outGroup.Length = 0 then
            [| group |]
        else
            [| group |] |> Array.append (groupCoordinates outGroup)

let calculateOuterEdges (cs: Coordinates) : Edges array =
    let coordinateGroups = cs |> groupCoordinates

    // TODO: accept Neighbors
    coordinateGroups
    |> Array.map (fun group ->
        group
        |> Array.map (fun (c, edgeTypes) ->
            let p = c |> toPixelCoordinate
            
            printf "position %f, %f" p.X p.Y

            edgeTypes |> Array.map (fun edgeType -> calculateEdge edgeType p))

            //let p = c |> toPixelCoordinate
            //let neighbors = c |> neighbors

            //edges
            //|> Array.map (fun edgeType ->

                // let edgeType = fst neighbor
                //let neighborCoordinate = snd neighbor

                //let hasNeighbor = group |> Array.contains neighborCoordinate

                //if hasNeighbor then None else Some(calculateEdge edgeType p))
            //|> Array.choose id)
        |> Array.collect id)

let drawAlongEachEdge (edges: Edges) (init) (draw) =
    if edges.Length > 0 then
        let firstEdge = edges[0]

        init (firstEdge |> fst)

        let rec loop (currentEdge: Edge) (index: int) =
            let endPos = currentEdge |> snd

            let nextEdge = edges |> Array.tryFind (fst >> isIdentical endPos)

            draw endPos

            match nextEdge with
            | Some edge when index < edges.Length -> loop edge (index + 1)
            | _ -> ()

        loop firstEdge 0

let drawEdges (ctx: CanvasRenderingContext2D) (color: string) (edges: Edges) =

    let draw = drawAlongEachEdge edges

    if edges.Length > 0 then

        ctx.beginPath ()
        ctx.globalAlpha <- 0.6
        ctx.fillStyle <- !^color

        draw (fun first -> ctx.moveTo (float first.X, float first.Y)) (fun pos -> ctx.lineTo (float pos.X, float pos.Y))

        ctx.closePath ()
        ctx.fill ()

        ctx.beginPath ()
        ctx.globalAlpha <- 1

        ctx.strokeStyle <- !^color
        ctx.lineWidth <- 1

        draw (fun first -> ctx.moveTo (float first.X, float first.Y)) (fun pos -> ctx.lineTo (float pos.X, float pos.Y))

        ctx.stroke ()
        ctx.closePath ()

let drawField (ctx: CanvasRenderingContext2D) (field: Field) =

    let draw = drawEdges ctx (color ())

    field.Coordinates |> calculateOuterEdges |> Array.map draw |> ignore


let canvas = document.getElementById "canvas" :?> HTMLCanvasElement
let ctx = canvas.getContext_2d ()

currentState.Fields 
|> Array.map (drawField ctx) 
|> ignore
