module Models

open HexMath

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

module Coordinate =
    let toPosition (c: Coordinate) =
        { X = (float c.X) * HexMath.r2 - ((float (c.Y % 2) * HexMath.r))
          Y = (float c.Y) * (HexMath.r2 - (HexMath.r - HexMath.rx)) }

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

module EdgeType = 
    let calculateEdge (edgeType: EdgeType) =
        match edgeType with
        | LeftTop -> (fun c -> { X = c.X; Y = c.Y - HexMath.r }, { X = c.X - HexMath.r; Y = c.Y - HexMath.rx })
        | Left -> (fun c -> { X = c.X - HexMath.r; Y = c.Y - HexMath.rx }, { X = c.X - HexMath.r; Y = c.Y + HexMath.rx })
        | LeftBottom -> (fun c -> { X = c.X - HexMath.r; Y = c.Y + HexMath.rx }, { X = c.X; Y = c.Y + HexMath.r })

        | RightBottom -> (fun c -> { X = c.X; Y = c.Y + HexMath.r }, { X = c.X + HexMath.r; Y = c.Y + HexMath.rx })
        | Right -> (fun c -> { X = c.X + HexMath.r; Y = c.Y + HexMath.rx }, { X = c.X + HexMath.r; Y = c.Y - HexMath.rx })
        | RightTop -> (fun c -> { X = c.X + HexMath.r; Y = c.Y - HexMath.rx }, { X = c.X; Y = c.Y - HexMath.r })

module Position =
    let allEdges = [| LeftTop; Left; LeftBottom; RightBottom; Right; RightTop |]

    let distance (pos1: Position) (pos2: Position) =
        let x = abs (pos1.X - pos2.X)
        let y = abs (pos1.Y - pos2.Y)

        sqrt (x * x + y * y)
    
    let calculateEdges (pos: Position) = 
         allEdges |> Array.map (fun edge -> EdgeType.calculateEdge edge pos)

type Neighbors = (Coordinate * (EdgeType array)) array
