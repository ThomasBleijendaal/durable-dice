import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { record_type, tuple_type, array_type, string_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { PositionModule_distance, EdgeType_$reflection, Position_$reflection, Coordinate_$reflection } from "./Models.fs.js";
import { HexMath_r } from "./HexMath.fs.js";

export class Hexagon extends Record {
    constructor(FieldId, Coordinate, Position, EdgeTypes, AllEdges, OuterEdges) {
        super();
        this.FieldId = FieldId;
        this.Coordinate = Coordinate;
        this.Position = Position;
        this.EdgeTypes = EdgeTypes;
        this.AllEdges = AllEdges;
        this.OuterEdges = OuterEdges;
    }
}

export function Hexagon_$reflection() {
    return record_type("Hexagon.Hexagon", [], Hexagon, () => [["FieldId", string_type], ["Coordinate", Coordinate_$reflection()], ["Position", Position_$reflection()], ["EdgeTypes", array_type(EdgeType_$reflection())], ["AllEdges", array_type(tuple_type(Position_$reflection(), Position_$reflection()))], ["OuterEdges", array_type(tuple_type(Position_$reflection(), Position_$reflection()))]]);
}

export class FieldHexagons extends Record {
    constructor(FieldId, Center, CenterPosition, Hexagons) {
        super();
        this.FieldId = FieldId;
        this.Center = Center;
        this.CenterPosition = CenterPosition;
        this.Hexagons = Hexagons;
    }
}

export function FieldHexagons_$reflection() {
    return record_type("Hexagon.FieldHexagons", [], FieldHexagons, () => [["FieldId", string_type], ["Center", Coordinate_$reflection()], ["CenterPosition", Position_$reflection()], ["Hexagons", array_type(array_type(Hexagon_$reflection()))]]);
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
    const dx = Math.abs(hex.Position.X - pos.X);
    const dy = Math.abs(hex.Position.Y - pos.Y);
    if ((dx < 1) && (dy < 1)) {
        return true;
    }
    else if ((dx > HexMath_r) ? true : (dy > HexMath_r)) {
        return false;
    }
    else {
        const distance = PositionModule_distance(pos, hex.Position);
        if (distance < HexMath_r) {
            return true;
        }
        else {
            const radToDeg = 3.141592653589793 / 180;
            const normalizeAngle = (angle_mut) => {
                normalizeAngle:
                while (true) {
                    const angle = angle_mut;
                    if (angle > (30 * radToDeg)) {
                        angle_mut = (angle - (60 * radToDeg));
                        continue normalizeAngle;
                    }
                    else {
                        return angle;
                    }
                    break;
                }
            };
            const angle_1 = Math.abs(normalizeAngle(Math.atan(dy / dx) * radToDeg));
            const maxDistance = HexMath_r / Math.cos(angle_1);
            return distance <= (maxDistance + 1);
        }
    }
}

