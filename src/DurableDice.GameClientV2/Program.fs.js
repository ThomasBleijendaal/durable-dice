import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { array_type, int32_type, union_type, record_type, float64_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { tryFind, contains, choose, map } from "./fable_modules/fable-library.4.11.0/Array.js";
import { equalArrays, round, safeHash, equals } from "./fable_modules/fable-library.4.11.0/Util.js";
import { nonSeeded } from "./fable_modules/fable-library.4.11.0/Random.js";

export class Coordinate extends Record {
    constructor(X, Y) {
        super();
        this.X = X;
        this.Y = Y;
    }
}

export function Coordinate_$reflection() {
    return record_type("Program.Coordinate", [], Coordinate, () => [["X", float64_type], ["Y", float64_type]]);
}

export class Position extends Record {
    constructor(X, Y) {
        super();
        this.X = X;
        this.Y = Y;
    }
}

export function Position_$reflection() {
    return record_type("Program.Position", [], Position, () => [["X", float64_type], ["Y", float64_type]]);
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
    return union_type("Program.EdgeType", [], EdgeType, () => [[], [], [], [], [], []]);
}

export class Field extends Record {
    constructor(Id, DiceCount, DiceAdded, Coordinates, Center) {
        super();
        this.Id = (Id | 0);
        this.DiceCount = (DiceCount | 0);
        this.DiceAdded = (DiceAdded | 0);
        this.Coordinates = Coordinates;
        this.Center = Center;
    }
}

export function Field_$reflection() {
    return record_type("Program.Field", [], Field, () => [["Id", int32_type], ["DiceCount", int32_type], ["DiceAdded", int32_type], ["Coordinates", array_type(Coordinate_$reflection())], ["Center", Coordinate_$reflection()]]);
}

export class GameState extends Record {
    constructor(Fields) {
        super();
        this.Fields = Fields;
    }
}

export function GameState_$reflection() {
    return record_type("Program.GameState", [], GameState, () => [["Fields", array_type(Field_$reflection())]]);
}

export const x = 1 / Math.sqrt(3);

export const r = 20;

export const r2 = r * 2;

export const rx = r * x;

export const allEdges = [new EdgeType(0, []), new EdgeType(1, []), new EdgeType(2, []), new EdgeType(3, []), new EdgeType(4, []), new EdgeType(5, [])];

export function toPixelCoordinate(c) {
    return new Position((c.X * r2) - ((c.Y % 2) * r), c.Y * (r2 - (r - rx)));
}

export const currentState = new GameState([new Field(1, 1, 0, [new Coordinate(5, 5), new Coordinate(5, 6), new Coordinate(7, 5), new Coordinate(4, 5), new Coordinate(4, 6), new Coordinate(2, 5), new Coordinate(6, 6), new Coordinate(8, 8)], new Coordinate(10, 10))]);

export function neighbors(c) {
    if ((c.Y % 2) === 1) {
        return [[new EdgeType(1, []), new Coordinate(c.X - 1, c.Y)], [new EdgeType(2, []), new Coordinate(c.X - 1, c.Y + 1)], [new EdgeType(3, []), new Coordinate(c.X, c.Y + 1)], [new EdgeType(4, []), new Coordinate(c.X + 1, c.Y)], [new EdgeType(5, []), new Coordinate(c.X, c.Y - 1)], [new EdgeType(0, []), new Coordinate(c.X - 1, c.Y - 1)]];
    }
    else {
        return [[new EdgeType(0, []), new Coordinate(c.X, c.Y - 1)], [new EdgeType(1, []), new Coordinate(c.X - 1, c.Y)], [new EdgeType(2, []), new Coordinate(c.X, c.Y + 1)], [new EdgeType(3, []), new Coordinate(c.X + 1, c.Y + 1)], [new EdgeType(4, []), new Coordinate(c.X + 1, c.Y)], [new EdgeType(5, []), new Coordinate(c.X + 1, c.Y - 1)]];
    }
}

export function calculateEdge(edge) {
    switch (edge.tag) {
        case 1:
            return (c_1) => [new Position(c_1.X - r, c_1.Y - rx), new Position(c_1.X - r, c_1.Y + rx)];
        case 2:
            return (c_2) => [new Position(c_2.X - r, c_2.Y + rx), new Position(c_2.X, c_2.Y + r)];
        case 3:
            return (c_3) => [new Position(c_3.X, c_3.Y + r), new Position(c_3.X + r, c_3.Y + rx)];
        case 4:
            return (c_4) => [new Position(c_4.X + r, c_4.Y + rx), new Position(c_4.X + r, c_4.Y - rx)];
        case 5:
            return (c_5) => [new Position(c_5.X + r, c_5.Y - rx), new Position(c_5.X, c_5.Y - r)];
        default:
            return (c) => [new Position(c.X, c.Y - r), new Position(c.X - r, c.Y - rx)];
    }
}

export function calculateEdges(p) {
    return map((edge) => calculateEdge(edge)(p), allEdges);
}

export function calculateOuterEdges(cs) {
    return map((c) => {
        const p = toPixelCoordinate(c);
        const neighbors_1 = neighbors(c);
        return choose((x_2) => x_2, map((neighbor) => {
            if (contains(neighbor[1], cs, {
                Equals: equals,
                GetHashCode: safeHash,
            })) {
                return void 0;
            }
            else {
                return calculateEdge(neighbor[0])(p);
            }
        }, neighbors_1));
    }, cs);
}

export function color() {
    const matchValue = ~~round(nonSeeded().NextDouble() * 8) | 0;
    switch (matchValue) {
        case 1:
            return "#ffc83d";
        case 2:
            return "#e3008c";
        case 3:
            return "#4f6bed";
        case 4:
            return "#ca5010";
        case 5:
            return "#00b294";
        case 6:
            return "#498205";
        case 7:
            return "#881798";
        default:
            return "#986f0b";
    }
}

export function drawEdges(ctx_1, color_1, edges) {
    if (edges.length > 0) {
        const firstEdge = edges[0];
        const loop = (currentEdge_mut) => {
            let edge;
            loop:
            while (true) {
                const currentEdge = currentEdge_mut;
                const startPos = currentEdge[0];
                const endPos = currentEdge[1];
                const nextEdge = tryFind((o) => equals(o[0], endPos), edges);
                ctx_1.lineTo(startPos.X, startPos.Y);
                ctx_1.lineTo(endPos.X, endPos.Y);
                let matchResult, edge_1;
                if (nextEdge != null) {
                    if ((edge = nextEdge, !equalArrays(firstEdge, edge))) {
                        matchResult = 0;
                        edge_1 = nextEdge;
                    }
                    else {
                        matchResult = 1;
                    }
                }
                else {
                    matchResult = 1;
                }
                switch (matchResult) {
                    case 0: {
                        currentEdge_mut = edge_1;
                        continue loop;
                        break;
                    }
                    case 1: {
                        break;
                    }
                }
                break;
            }
        };
        ctx_1.beginPath();
        ctx_1.globalAlpha = 0.6;
        ctx_1.fillStyle = color_1;
        const first = firstEdge[0];
        ctx_1.moveTo(first.X, first.Y);
        loop(firstEdge);
        ctx_1.fill();
        ctx_1.closePath();
    }
}

export function drawEdgesOld(ctx_1, color_1, edges) {
    if (edges.length > 0) {
        ctx_1.beginPath();
        ctx_1.globalAlpha = 0.6;
        ctx_1.fillStyle = color_1;
        const first = edges[0][0];
        ctx_1.moveTo(first.X, first.Y);
        const arr = map((tuple_1) => tuple_1[0], edges);
        for (let idx = 0; idx <= (arr.length - 1); idx++) {
            const edge = arr[idx];
            ctx_1.lineTo(edge.X, edge.Y);
        }
        ctx_1.fill();
        ctx_1.closePath();
        ctx_1.beginPath();
        ctx_1.globalAlpha = 1;
        ctx_1.strokeStyle = color_1;
        ctx_1.lineWidth = 1;
        ctx_1.moveTo(first.X, first.Y);
        const arr_1 = map((tuple_2) => tuple_2[0], edges);
        for (let idx_1 = 0; idx_1 <= (arr_1.length - 1); idx_1++) {
            const edge_1 = arr_1[idx_1];
            ctx_1.lineTo(edge_1.X, edge_1.Y);
        }
        ctx_1.lineTo(first.X, first.Y);
        ctx_1.stroke();
        ctx_1.closePath();
    }
}

export function drawField(ctx_1, field) {
    let draw;
    const color_1 = color();
    draw = ((edges) => {
        drawEdges(ctx_1, color_1, edges);
    });
    let drawOld;
    const color_2 = color();
    drawOld = ((edges_1) => {
        drawEdgesOld(ctx_1, color_2, edges_1);
    });
    map(drawOld, map((arg) => calculateEdges(toPixelCoordinate(arg)), field.Coordinates));
    map(draw, calculateOuterEdges(field.Coordinates));
    return (value_2) => {
    };
}

export const canvas = document.getElementById("canvas");

export const ctx = canvas.getContext('2d');

map((field) => drawField(ctx, field), currentState.Fields);

