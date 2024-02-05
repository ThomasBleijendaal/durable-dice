import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { string_type, record_type, tuple_type, array_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { PositionModule_distance, EdgeType_$reflection, Position_$reflection, Coordinate_$reflection } from "./Models.fs.js";
import { HexMath_r } from "./HexMath.fs.js";

export class Hexagon extends Record {
    constructor(FieldId, Coordinate, Position, EdgeTypes, AllEdges, OuterEdges) {
        super();
        this.FieldId = (FieldId | 0);
        this.Coordinate = Coordinate;
        this.Position = Position;
        this.EdgeTypes = EdgeTypes;
        this.AllEdges = AllEdges;
        this.OuterEdges = OuterEdges;
    }
}

export function Hexagon_$reflection() {
    return record_type("Hexagon.Hexagon", [], Hexagon, () => [["FieldId", int32_type], ["Coordinate", Coordinate_$reflection()], ["Position", Position_$reflection()], ["EdgeTypes", array_type(EdgeType_$reflection())], ["AllEdges", array_type(tuple_type(Position_$reflection(), Position_$reflection()))], ["OuterEdges", array_type(tuple_type(Position_$reflection(), Position_$reflection()))]]);
}

export class FieldHexagons extends Record {
    constructor(FieldId, Center, CenterPosition, Color, Hexagons) {
        super();
        this.FieldId = (FieldId | 0);
        this.Center = Center;
        this.CenterPosition = CenterPosition;
        this.Color = Color;
        this.Hexagons = Hexagons;
    }
}

export function FieldHexagons_$reflection() {
    return record_type("Hexagon.FieldHexagons", [], FieldHexagons, () => [["FieldId", int32_type], ["Center", Coordinate_$reflection()], ["CenterPosition", Position_$reflection()], ["Color", string_type], ["Hexagons", array_type(array_type(Hexagon_$reflection()))]]);
}

export function HexagonModule_coordinate(hex) {
    return hex.Coordinate;
}

export function HexagonModule_position(hex) {
    return hex.Position;
}

export function HexagonModule_outerEdges(hex) {
    return hex.OuterEdges;
}

export function HexagonModule_isInside(pos, hex) {
    return PositionModule_distance(pos, hex.Position) < HexMath_r;
}

