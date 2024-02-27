module Hexagon

open HexMath
open Models
open System

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

        let dx = abs (hex.Position.X - pos.X)
        let dy = abs (hex.Position.Y - pos.Y)

        if dx < 1.0<px> && dy < 1.0<px> then
            true // close enough
        else if dx > HexMath.r || dy > HexMath.r then
            false // far enough
        else
            let distance = Position.distance pos hex.Position

            if distance < HexMath.r then
                true // in circle within hexagon
            else
                let radToDeg = Math.PI / 180.0

                let rec normalizeAngle (angle) =
                    if angle > (30.0 * radToDeg) then
                        normalizeAngle (angle - (60.0 * radToDeg))
                    else
                        angle

                let angle = abs (normalizeAngle (atan (dy / dx) * radToDeg))

                let maxDistance = HexMath.r / (cos angle)

                distance <= (maxDistance + 1.0<px>)

