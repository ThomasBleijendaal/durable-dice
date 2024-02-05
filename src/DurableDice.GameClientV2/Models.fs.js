import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { union_type, float64_type, record_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { HexMath_rx, HexMath_r, HexMath_r2 } from "./HexMath.fs.js";
import { map } from "./fable_modules/fable-library.4.11.0/Array.js";

export class Coordinate extends Record {
    constructor(X, Y) {
        super();
        this.X = (X | 0);
        this.Y = (Y | 0);
    }
}

export function Coordinate_$reflection() {
    return record_type("Models.Coordinate", [], Coordinate, () => [["X", int32_type], ["Y", int32_type]]);
}

export class Position extends Record {
    constructor(X, Y) {
        super();
        this.X = X;
        this.Y = Y;
    }
}

export function Position_$reflection() {
    return record_type("Models.Position", [], Position, () => [["X", float64_type], ["Y", float64_type]]);
}

export class EdgeType extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["LeftTop", "Left", "LeftBottom", "RightBottom", "Right", "RightTop"];
    }
}

export function EdgeType_$reflection() {
    return union_type("Models.EdgeType", [], EdgeType, () => [[], [], [], [], [], []]);
}

export function CoordinateModule_toPosition(c) {
    return new Position((c.X * HexMath_r2) - ((c.Y % 2) * HexMath_r), c.Y * (HexMath_r2 - (HexMath_r - HexMath_rx)));
}

export function CoordinateModule_neighbors(c) {
    if ((c.Y % 2) === 1) {
        return [[new EdgeType(1, []), new Coordinate(c.X - 1, c.Y)], [new EdgeType(2, []), new Coordinate(c.X - 1, c.Y + 1)], [new EdgeType(3, []), new Coordinate(c.X, c.Y + 1)], [new EdgeType(4, []), new Coordinate(c.X + 1, c.Y)], [new EdgeType(5, []), new Coordinate(c.X, c.Y - 1)], [new EdgeType(0, []), new Coordinate(c.X - 1, c.Y - 1)]];
    }
    else {
        return [[new EdgeType(0, []), new Coordinate(c.X, c.Y - 1)], [new EdgeType(1, []), new Coordinate(c.X - 1, c.Y)], [new EdgeType(2, []), new Coordinate(c.X, c.Y + 1)], [new EdgeType(3, []), new Coordinate(c.X + 1, c.Y + 1)], [new EdgeType(4, []), new Coordinate(c.X + 1, c.Y)], [new EdgeType(5, []), new Coordinate(c.X + 1, c.Y - 1)]];
    }
}

export function EdgeTypeModule_calculateEdge(edgeType) {
    switch (edgeType.tag) {
        case 1:
            return (c_1) => [new Position(c_1.X - HexMath_r, c_1.Y - HexMath_rx), new Position(c_1.X - HexMath_r, c_1.Y + HexMath_rx)];
        case 2:
            return (c_2) => [new Position(c_2.X - HexMath_r, c_2.Y + HexMath_rx), new Position(c_2.X, c_2.Y + HexMath_r)];
        case 3:
            return (c_3) => [new Position(c_3.X, c_3.Y + HexMath_r), new Position(c_3.X + HexMath_r, c_3.Y + HexMath_rx)];
        case 4:
            return (c_4) => [new Position(c_4.X + HexMath_r, c_4.Y + HexMath_rx), new Position(c_4.X + HexMath_r, c_4.Y - HexMath_rx)];
        case 5:
            return (c_5) => [new Position(c_5.X + HexMath_r, c_5.Y - HexMath_rx), new Position(c_5.X, c_5.Y - HexMath_r)];
        default:
            return (c) => [new Position(c.X, c.Y - HexMath_r), new Position(c.X - HexMath_r, c.Y - HexMath_rx)];
    }
}

export const PositionModule_allEdges = [new EdgeType(0, []), new EdgeType(1, []), new EdgeType(2, []), new EdgeType(3, []), new EdgeType(4, []), new EdgeType(5, [])];

export function PositionModule_distance(pos1, pos2) {
    const x = Math.abs(pos1.X - pos2.X);
    const y = Math.abs(pos1.Y - pos2.Y);
    return Math.sqrt((x * x) + (y * y));
}

export function PositionModule_calculateEdges(pos) {
    return map((edge) => EdgeTypeModule_calculateEdge(edge)(pos), PositionModule_allEdges);
}

