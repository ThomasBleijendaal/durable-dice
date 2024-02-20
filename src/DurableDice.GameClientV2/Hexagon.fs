module Hexagon

open HexMath
open Models

type Hexagon = {
    FieldId: string

    Coordinate: Coordinate
    Position: Position
    EdgeTypes: EdgeType array
    AllEdges: Edges
    OuterEdges: Edges
}

type HexagonGroup = Hexagon array

type FieldHexagons = {
    FieldId: string
    Center: Coordinate
    CenterPosition: Position

    Hexagons: HexagonGroup array }

module Hexagon =
    let coordinate (hex: Hexagon) = hex.Coordinate
    let position (hex: Hexagon) = hex.Position
    let outerEdges (hex: Hexagon) = hex.OuterEdges
    let isInside (pos: Position) (hex: Hexagon) =
        // TODO: apply correct math instead of circle
        Position.distance pos hex.Position < HexMath.r

