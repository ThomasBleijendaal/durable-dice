import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { array_type, union_type, float64_type, record_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { curry3, safeHash, equals, round } from "./fable_modules/fable-library.4.11.0/Util.js";
import { nonSeeded } from "./fable_modules/fable-library.4.11.0/Random.js";
import { take, append, collect, contains, map } from "./fable_modules/fable-library.4.11.0/Array.js";
import { Array_distinct } from "./fable_modules/fable-library.4.11.0/Seq2.js";
import { printf, toConsole } from "./fable_modules/fable-library.4.11.0/String.js";

export class Coordinate extends Record {
    constructor(X, Y) {
        super();
        this.X = (X | 0);
        this.Y = (Y | 0);
    }
}

export function Coordinate_$reflection() {
    return record_type("Program.Coordinate", [], Coordinate, () => [["X", int32_type], ["Y", int32_type]]);
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

export function toPosition(c) {
    return new Position((c.X * r2) - ((c.Y % 2) * r), c.Y * (r2 - (r - rx)));
}

export const currentState = new GameState([new Field(1, 1, 0, [new Coordinate(5, 4), new Coordinate(5, 5), new Coordinate(6, 6), new Coordinate(7, 5), new Coordinate(6, 4), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 2), new Coordinate(3, 3), new Coordinate(2, 4), new Coordinate(4, 5), new Coordinate(3, 6), new Coordinate(5, 6)], new Coordinate(7, 5)), new Field(2, 2, 0, [new Coordinate(12, 12), new Coordinate(11, 12), new Coordinate(10, 12), new Coordinate(11, 11), new Coordinate(12, 11), new Coordinate(13, 12), new Coordinate(14, 12), new Coordinate(14, 13)], new Coordinate(12, 12)), new Field(3, 2, 0, [new Coordinate(7, 6), new Coordinate(8, 5), new Coordinate(8, 6), new Coordinate(9, 7), new Coordinate(9, 5), new Coordinate(10, 6), new Coordinate(10, 7), new Coordinate(10, 8)], new Coordinate(12, 12))]);

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

export function groupNeighbors(cs) {
    if (cs.length === 0) {
        return [];
    }
    else {
        const loop = (processedCoordinates, nextCoordinates) => {
            const coordinateNeighbors = map((coordinate) => {
                const neighborsOfCoordinate = neighbors(coordinate);
                const externalEdges = map((tuple) => tuple[0], neighborsOfCoordinate.filter((tupledArg) => {
                    const c_1 = tupledArg[1];
                    return contains(c_1, cs, {
                        Equals: equals,
                        GetHashCode: safeHash,
                    }) === false;
                }));
                const unprocessedNeighborsOfCoordinate = map((tuple_1) => tuple_1[1], neighborsOfCoordinate.filter((tupledArg_1) => {
                    const c_2 = tupledArg_1[1];
                    if (contains(c_2, processedCoordinates, {
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
                }));
                return [[coordinate, externalEdges], unprocessedNeighborsOfCoordinate];
            }, nextCoordinates);
            const coordinateEdges = map((tuple_2) => tuple_2[0], coordinateNeighbors);
            const nextNeighborsToProcess = Array_distinct(collect((x_4) => x_4, map((tuple_3) => tuple_3[1], coordinateNeighbors)), {
                Equals: equals,
                GetHashCode: safeHash,
            });
            const processedCoordinates_1 = append(nextNeighborsToProcess, append(map((tuple_4) => tuple_4[0], coordinateEdges), processedCoordinates));
            if (nextNeighborsToProcess.length === 0) {
                return coordinateEdges;
            }
            else {
                return append(loop(processedCoordinates_1, nextNeighborsToProcess), coordinateEdges);
            }
        };
        const firstCoordinate = [cs[0]];
        const coordinatesWithEdges = loop(firstCoordinate, firstCoordinate);
        const outGroup = cs.filter((c_3) => (contains(c_3, map((tuple_5) => tuple_5[0], coordinatesWithEdges), {
            Equals: equals,
            GetHashCode: safeHash,
        }) === false));
        if (outGroup.length === 0) {
            return [coordinatesWithEdges];
        }
        else {
            const array_22 = [coordinatesWithEdges];
            return append(groupNeighbors(outGroup), array_22);
        }
    }
}

export function calculateOuterEdges(neighborGroups) {
    return map((group) => collect((x_1) => x_1, map((tupledArg) => {
        const c = tupledArg[0];
        const edgeTypes = tupledArg[1];
        const p = toPosition(c);
        return map((edgeType) => calculateEdge(edgeType)(p), edgeTypes);
    }, group)), neighborGroups);
}

export function calculatePositions(neighborGroups) {
    return map((group) => map((tupledArg) => {
        const c = tupledArg[0];
        return toPosition(c);
    }, group), neighborGroups);
}

export function drawLine(ctx_1, color_1, _arg1_, _arg1__1) {
    const _arg = [_arg1_, _arg1__1];
    const p2 = _arg[1];
    const p1 = _arg[0];
    ctx_1.beginPath();
    ctx_1.globalAlpha = 1;
    ctx_1.lineWidth = 1;
    ctx_1.strokeStyle = color_1;
    ctx_1.moveTo(p1.X, p1.Y);
    ctx_1.lineTo(p2.X, p2.Y);
    ctx_1.stroke();
    ctx_1.closePath();
}

export function drawGlowingLine(ctx_1, color_1, _arg1_, _arg1__1) {
    const _arg = [_arg1_, _arg1__1];
    const p2 = _arg[1];
    const p1 = _arg[0];
    ctx_1.beginPath();
    ctx_1.globalAlpha = 1;
    ctx_1.lineWidth = 0;
    ctx_1.shadowBlur = 5;
    ctx_1.shadowColor = "white";
    ctx_1.moveTo(p1.X, p1.Y);
    ctx_1.lineTo(p2.X, p2.Y);
    ctx_1.stroke();
    ctx_1.closePath();
}

export function drawAllEdges(ctx_1, color_1, drawLine_1, edges) {
    const drawLine_2 = curry3(drawLine_1)(ctx_1)(color_1);
    edges.forEach(drawLine_2);
}

export function drawHexagons(ctx_1, color_1, positions) {
    positions.forEach((position) => {
        const edges = calculateEdges(position);
        const firstEdge = edges[0][0];
        ctx_1.beginPath();
        ctx_1.fillStyle = color_1;
        ctx_1.lineWidth = 0.2;
        ctx_1.strokeStyle = color_1;
        ctx_1.globalAlpha = 0.6;
        ctx_1.moveTo(firstEdge.X, firstEdge.Y);
        for (let idx = 0; idx <= (edges.length - 1); idx++) {
            const edge = edges[idx][1];
            ctx_1.lineTo(edge.X, edge.Y);
        }
        ctx_1.closePath();
        ctx_1.stroke();
        ctx_1.fill();
    });
}

export function drawCoordinate(ctx_1, c) {
    ctx_1.globalAlpha = 1;
    ctx_1.strokeStyle = "white";
    ctx_1.fillStyle = "white";
    const pos = toPosition(c);
    ctx_1.fillText(`${c.X},${c.Y}`, pos.X, pos.Y);
}

export function drawField(ctx_1, field) {
    const color_1 = color();
    const drawHexagons_1 = (positions) => {
        drawHexagons(ctx_1, color_1, positions);
    };
    const drawEdges = (edges) => {
        drawAllEdges(ctx_1, color_1, (ctx_2, color_2, tupledArg) => {
            drawLine(ctx_2, color_2, tupledArg[0], tupledArg[1]);
        }, edges);
    };
    const drawOutline = (edges_1) => {
        drawAllEdges(ctx_1, color_1, (ctx_3, color_3, tupledArg_1) => {
            drawGlowingLine(ctx_3, color_3, tupledArg_1[0], tupledArg_1[1]);
        }, edges_1);
    };
    const neighborGroups = groupNeighbors(field.Coordinates);
    const neighborGroupPositions = calculatePositions(neighborGroups);
    const outerEdges = calculateOuterEdges(neighborGroups);
    neighborGroupPositions.forEach(drawHexagons_1);
    outerEdges.forEach(drawEdges);
    outerEdges.forEach(drawOutline);
}

export function mousemove(event) {
    const arg = event.x;
    const arg_1 = event.y;
    toConsole(printf("Mouse is at %f, %f"))(arg)(arg_1);
}

export const canvas = document.getElementById("canvas");

export const ctx = canvas.getContext('2d');

map((field) => {
    drawField(ctx, field);
}, take(1, currentState.Fields));

