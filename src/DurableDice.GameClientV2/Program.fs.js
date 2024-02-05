import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { FieldModule_groupHexagons, Field, Field_$reflection } from "./Field.fs.js";
import { class_type, record_type, array_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { defaultOf, createAtom } from "./fable_modules/fable-library.4.11.0/Util.js";
import { collect, map, tryFind } from "./fable_modules/fable-library.4.11.0/Array.js";
import { Position, Coordinate } from "./Models.fs.js";
import { HexagonModule_isInside } from "./Hexagon.fs.js";

export class GameState extends Record {
    constructor(Fields) {
        super();
        this.Fields = Fields;
    }
}

export function GameState_$reflection() {
    return record_type("Program.GameState", [], GameState, () => [["Fields", array_type(Field_$reflection())]]);
}

export let selectedField = createAtom(void 0);

export class DefaultAnimation {
    constructor() {
    }
    Applies(_arg) {
        return true;
    }
    get IsDone() {
        return false;
    }
    AnimateHexagon(ctx, hexagons, hexagon) {
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        ctx.beginPath();
        ctx.fillStyle = hexagons.Color;
        ctx.lineWidth = 0.2;
        ctx.strokeStyle = hexagons.Color;
        ctx.globalAlpha = 0.6;
        ctx.shadowBlur = 0;
        ctx.shadowColor = defaultOf();
        ctx.moveTo(firstEdge.X, firstEdge.Y);
        for (let idx = 0; idx <= (edges.length - 1); idx++) {
            const edge = edges[idx][1];
            ctx.lineTo(edge.X, edge.Y);
        }
        ctx.closePath();
        ctx.stroke();
        ctx.fill();
    }
    AnimateOuterEdge(ctx, hexagons, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 1;
            ctx.strokeStyle = hexagons.Color;
            ctx.shadowBlur = 0;
            ctx.shadowColor = defaultOf();
            ctx.moveTo(p1.X, p1.Y);
            ctx.lineTo(p2.X, p2.Y);
            ctx.stroke();
            ctx.closePath();
        });
    }
}

export function DefaultAnimation_$reflection() {
    return class_type("Program.DefaultAnimation", void 0, DefaultAnimation);
}

export function DefaultAnimation_$ctor() {
    return new DefaultAnimation();
}

export class SelectedAnimation {
    constructor() {
    }
    Applies(hexagon) {
        if (selectedField() == null) {
            return false;
        }
        else {
            const field = selectedField();
            return field.Id === hexagon.FieldId;
        }
    }
    get IsDone() {
        return false;
    }
    AnimateHexagon(ctx, hexagons, hexagon) {
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        ctx.beginPath();
        ctx.fillStyle = hexagons.Color;
        ctx.lineWidth = 1;
        ctx.strokeStyle = hexagons.Color;
        ctx.globalAlpha = 0.8;
        ctx.shadowBlur = 0;
        ctx.shadowColor = defaultOf();
        ctx.moveTo(firstEdge.X, firstEdge.Y);
        for (let idx = 0; idx <= (edges.length - 1); idx++) {
            const edge = edges[idx][1];
            ctx.lineTo(edge.X, edge.Y);
        }
        ctx.closePath();
        ctx.stroke();
        ctx.fill();
    }
    AnimateOuterEdge(ctx, _arg, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 2;
            ctx.strokeStyle = "white";
            ctx.shadowBlur = 5;
            ctx.shadowColor = "white";
            ctx.moveTo(p1.X, p1.Y);
            ctx.lineTo(p2.X, p2.Y);
            ctx.stroke();
            ctx.closePath();
        });
    }
}

export function SelectedAnimation_$reflection() {
    return class_type("Program.SelectedAnimation", void 0, SelectedAnimation);
}

export function SelectedAnimation_$ctor() {
    return new SelectedAnimation();
}

export class CapturedAnimation {
    constructor(fieldId) {
        this.fieldId = (fieldId | 0);
        this.progress = 0;
        this.max = 1000;
    }
    Applies(hexagon) {
        const this$ = this;
        return hexagon.FieldId === this$.fieldId;
    }
    get IsDone() {
        const this$ = this;
        this$.progress = (this$.progress + 1);
        return this$.progress > this$.max;
    }
    AnimateHexagon(ctx, hexagons, hexagon) {
        const this$ = this;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagon.Position.X, hexagon.Position.Y, 500);
        gradient.addColorStop(1 - (this$.progress / this$.max), hexagons.Color);
        gradient.addColorStop(0, "lime");
        ctx.beginPath();
        ctx.fillStyle = gradient;
        ctx.lineWidth = 0.2;
        ctx.strokeStyle = gradient;
        ctx.globalAlpha = 0.6;
        ctx.shadowBlur = 0;
        ctx.shadowColor = defaultOf();
        ctx.moveTo(firstEdge.X, firstEdge.Y);
        for (let idx = 0; idx <= (edges.length - 1); idx++) {
            const edge = edges[idx][1];
            ctx.lineTo(edge.X, edge.Y);
        }
        ctx.closePath();
        ctx.stroke();
        ctx.fill();
    }
    AnimateOuterEdge(ctx, hexagons, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 1;
            ctx.strokeStyle = hexagons.Color;
            ctx.shadowBlur = 0;
            ctx.shadowColor = defaultOf();
            ctx.moveTo(p1.X, p1.Y);
            ctx.lineTo(p2.X, p2.Y);
            ctx.stroke();
            ctx.closePath();
        });
    }
}

export function CapturedAnimation_$reflection() {
    return class_type("Program.CapturedAnimation", void 0, CapturedAnimation);
}

export function CapturedAnimation_$ctor_Z524259A4(fieldId) {
    return new CapturedAnimation(fieldId);
}

export const defaultAnimation = DefaultAnimation_$ctor();

export const selectedAnimation = SelectedAnimation_$ctor();

export let runningAnimations = createAtom([]);

export function Animation_apply(method, ctx, color, hexagon) {
    let array_1;
    const runningAnimation = tryFind((animation) => animation.Applies(hexagon), runningAnimations());
    if (runningAnimation == null) {
        if (selectedAnimation.Applies(hexagon)) {
            method(selectedAnimation, [ctx, color, hexagon]);
        }
        else {
            method(defaultAnimation, [ctx, color, hexagon]);
        }
    }
    else {
        const animation_1 = runningAnimation;
        method(animation_1, [ctx, color, hexagon]);
    }
    if (runningAnimation != null) {
        runningAnimations((array_1 = runningAnimations(), array_1.filter((animation_2) => (animation_2.IsDone === false))));
    }
}

export let capturedField = createAtom(void 0);

export const currentState = new GameState([new Field(1, 0, 1, 0, [new Coordinate(5, 4), new Coordinate(5, 5), new Coordinate(6, 6), new Coordinate(7, 5), new Coordinate(6, 4), new Coordinate(5, 7), new Coordinate(3, 1), new Coordinate(3, 2), new Coordinate(3, 3), new Coordinate(2, 4), new Coordinate(4, 5), new Coordinate(3, 6), new Coordinate(5, 6)], new Coordinate(5, 6)), new Field(2, 1, 2, 0, [new Coordinate(12, 12), new Coordinate(11, 12), new Coordinate(10, 12), new Coordinate(11, 11), new Coordinate(12, 11), new Coordinate(13, 12), new Coordinate(14, 12), new Coordinate(14, 13)], new Coordinate(12, 11)), new Field(3, 2, 2, 0, [new Coordinate(7, 6), new Coordinate(8, 5), new Coordinate(8, 6), new Coordinate(9, 7), new Coordinate(9, 5), new Coordinate(10, 6), new Coordinate(10, 7), new Coordinate(10, 8)], new Coordinate(9, 5))]);

export function drawFieldHexagons(ctx, hexagons) {
    const arr = hexagons.Hexagons;
    for (let idx = 0; idx <= (arr.length - 1); idx++) {
        const group = arr[idx];
        for (let idx_1 = 0; idx_1 <= (group.length - 1); idx_1++) {
            const hexagon = group[idx_1];
            Animation_apply((animation, tupledArg) => {
                animation.AnimateHexagon(tupledArg[0], tupledArg[1], tupledArg[2]);
            }, ctx, hexagons, hexagon);
        }
    }
    const arr_1 = hexagons.Hexagons;
    for (let idx_2 = 0; idx_2 <= (arr_1.length - 1); idx_2++) {
        const group_1 = arr_1[idx_2];
        for (let idx_3 = 0; idx_3 <= (group_1.length - 1); idx_3++) {
            const hexagon_1 = group_1[idx_3];
            Animation_apply((animation_1, tupledArg_1) => {
                animation_1.AnimateOuterEdge(tupledArg_1[0], tupledArg_1[1], tupledArg_1[2]);
            }, ctx, hexagons, hexagon_1);
        }
    }
}

export const currentFieldHexagons = map(FieldModule_groupHexagons, currentState.Fields);

capturedField(currentState.Fields[0]);

export const allHexagons = collect((x_1) => x_1, map((hexGroups) => collect((x) => x, hexGroups.Hexagons), currentFieldHexagons));

export const canvas = document.getElementById("canvas");

export function selectField(foundHexagon) {
    let hex, field_1;
    const findField = (id) => tryFind((field) => (field.Id === id), currentState.Fields);
    const matchValue = selectedField();
    let matchResult, hex_1, field_2, hex_2;
    if (foundHexagon != null) {
        if (matchValue != null) {
            if ((hex = foundHexagon, (field_1 = matchValue, hex.FieldId !== field_1.Id))) {
                matchResult = 1;
                field_2 = matchValue;
                hex_2 = foundHexagon;
            }
            else {
                matchResult = 2;
            }
        }
        else {
            matchResult = 0;
            hex_1 = foundHexagon;
        }
    }
    else {
        matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            selectedField(findField(hex_1.FieldId));
            break;
        }
        case 1: {
            selectedField(findField(hex_2.FieldId));
            break;
        }
        case 2: {
            selectedField(void 0);
            break;
        }
    }
}

export function mouseclick(event) {
    const rect = canvas.getBoundingClientRect();
    const pos = new Position((event.x - rect.left) * 1, (event.y - rect.top) * 1);
    const isHover = (hex) => HexagonModule_isInside(pos, hex);
    const foundHexagon = tryFind(isHover, allHexagons);
    if (foundHexagon == null) {
    }
    else {
        const hexagon = foundHexagon;
        runningAnimations([CapturedAnimation_$ctor_Z524259A4(hexagon.FieldId)]);
    }
}

canvas.onclick = ((event) => {
    mouseclick(event);
});

export let previousTime = createAtom(0);

export function draw(time) {
    const delta = time - previousTime();
    if (delta > 10) {
        previousTime(time);
        const w = canvas.width;
        const h = canvas.height;
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, w, h);
        for (let idx = 0; idx <= (currentFieldHexagons.length - 1); idx++) {
            const group = currentFieldHexagons[idx];
            drawFieldHexagons(ctx, group);
        }
    }
    return window.requestAnimationFrame((arg) => {
        draw(arg);
    });
}

draw(0);

