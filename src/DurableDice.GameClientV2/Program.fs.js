import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { array_type, int32_type, union_type, record_type, float64_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { safeHash, equals, round } from "./fable_modules/fable-library.4.11.0/Util.js";
import { nonSeeded } from "./fable_modules/fable-library.4.11.0/Random.js";
import { tryFind, choose, append, contains, collect, map } from "./fable_modules/fable-library.4.11.0/Array.js";
import { Array_except } from "./fable_modules/fable-library.4.11.0/Seq2.js";

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

export const x = round(1000 / Math.sqrt(3)) / 1000;

export const r = 30;

export const r2 = r * 2;

export const rx = r * x;

export const allEdges = [new EdgeType(0, []), new EdgeType(1, []), new EdgeType(2, []), new EdgeType(3, []), new EdgeType(4, []), new EdgeType(5, [])];

export function isIdentical(e1, e2) {
    if (Math.abs(e1.X - e2.X) < 0.1) {
        return Math.abs(e1.Y - e2.Y) < 0.1;
    }
    else {
        return false;
    }
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

export function toPixelCoordinate(c) {
    return new Position((c.X * r2) - ((c.Y % 2) * r), c.Y * (r2 - (r - rx)));
}

export const currentState = new GameState([new Field(1, 1, 0, [new Coordinate(5, 5), new Coordinate(5, 6), new Coordinate(7, 5), new Coordinate(4, 5), new Coordinate(4, 6), new Coordinate(2, 5), new Coordinate(6, 6), new Coordinate(8, 8)], new Coordinate(7, 5)), new Field(2, 2, 0, [new Coordinate(12, 12), new Coordinate(11, 12), new Coordinate(10, 12), new Coordinate(11, 11), new Coordinate(12, 11), new Coordinate(13, 12), new Coordinate(14, 12), new Coordinate(14, 13)], new Coordinate(12, 12))]);

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

export function groupCoordinates(cs) {
    if (cs.length === 0) {
        return [];
    }
    else {
        const firstCoordinate = [cs[0]];
        const loop = (group_mut) => {
            loop:
            while (true) {
                const group = group_mut;
                let groupNeighbors;
                const array_5 = collect((x_1) => x_1, map((c) => map((tuple) => tuple[1], neighbors(c)), group));
                groupNeighbors = array_5.filter((c_2) => {
                    if (contains(c_2, group, {
                        Equals: equals,
                        GetHashCode: safeHash,
                    }) === false) {
                        return contains(c_2, cs, {
                            Equals: equals,
                            GetHashCode: safeHash,
                        }) === true;
                    }
                    else {
                        return false;
                    }
                });
                if (groupNeighbors.length === 0) {
                    return group;
                }
                else {
                    group_mut = append(groupNeighbors, group);
                    continue loop;
                }
                break;
            }
        };
        const group_1 = loop(firstCoordinate);
        const outGroup = Array_except(group_1, cs, {
            Equals: equals,
            GetHashCode: safeHash,
        });
        if (outGroup.length === 0) {
            return [group_1];
        }
        else {
            const array_9 = [group_1];
            return append(groupCoordinates(outGroup), array_9);
        }
    }
}

export function calculateOuterEdges(cs) {
    const coordinateGroups = groupCoordinates(cs);
    return map((group) => collect((x_3) => x_3, map((c) => {
        const p = toPixelCoordinate(c);
        const neighbors_1 = neighbors(c);
        return choose((x_2) => x_2, map((neighbor) => {
            const edgeType = neighbor[0];
            const neighborCoordinate = neighbor[1];
            const hasNeighbor = contains(neighborCoordinate, group, {
                Equals: equals,
                GetHashCode: safeHash,
            });
            if (hasNeighbor) {
                return void 0;
            }
            else {
                return calculateEdge(edgeType)(p);
            }
        }, neighbors_1));
    }, group)), coordinateGroups);
}

export function drawAlongEachEdge(edges, init, draw) {
    if (edges.length > 0) {
        const firstEdge = edges[0];
        init(firstEdge[0]);
        const loop = (currentEdge_mut, index_mut) => {
            let edge;
            loop:
            while (true) {
                const currentEdge = currentEdge_mut, index = index_mut;
                const endPos = currentEdge[1];
                const nextEdge = tryFind((arg) => isIdentical(endPos, arg[0]), edges);
                draw(endPos);
                let matchResult, edge_1;
                if (nextEdge != null) {
                    if ((edge = nextEdge, index < edges.length)) {
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
                        index_mut = (index + 1);
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
        loop(firstEdge, 0);
    }
}

export function drawEdges(ctx_1, color_1, edges) {
    const draw_1 = (init, draw) => {
        drawAlongEachEdge(edges, init, draw);
    };
    if (edges.length > 0) {
        ctx_1.beginPath();
        ctx_1.globalAlpha = 0.6;
        ctx_1.fillStyle = color_1;
        draw_1((first) => {
            ctx_1.moveTo(first.X, first.Y);
        }, (pos) => {
            ctx_1.lineTo(pos.X, pos.Y);
        });
        ctx_1.closePath();
        ctx_1.fill();
        ctx_1.beginPath();
        ctx_1.globalAlpha = 1;
        ctx_1.strokeStyle = color_1;
        ctx_1.lineWidth = 1;
        draw_1((first_1) => {
            ctx_1.moveTo(first_1.X, first_1.Y);
        }, (pos_1) => {
            ctx_1.lineTo(pos_1.X, pos_1.Y);
        });
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
    map(draw, calculateOuterEdges(field.Coordinates));
}

export const canvas = document.getElementById("canvas");

export const ctx = canvas.getContext('2d');

map((field) => {
    drawField(ctx, field);
}, currentState.Fields);

