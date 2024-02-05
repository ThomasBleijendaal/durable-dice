module Hexagon

open HexMath
open Models

type Hexagon = {
    FieldId: int

    Coordinate: Coordinate
    Position: Position
    EdgeTypes: EdgeType array
    AllEdges: Edges
    OuterEdges: Edges
}

type HexagonGroup = Hexagon array

type FieldHexagons = {
    FieldId: int
    Center: Coordinate
    CenterPosition: Position

    Color: string

    Hexagons: HexagonGroup array }

module Hexagon =
    let coordinate (hex: Hexagon) = hex.Coordinate
    let position (hex: Hexagon) = hex.Position
    let outerEdges (hex: Hexagon) = hex.OuterEdges
    let isInside (pos: Position) (hex: Hexagon) =
        Position.distance pos hex.Position < HexMath.r

