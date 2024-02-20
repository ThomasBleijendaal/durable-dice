import { GameStateModule_selectedField, GameStateModule_hoverField } from "./GameState.fs.js";
import { createAtom, defaultOf } from "./fable_modules/fable-library.4.11.0/Util.js";
import { class_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { tryFind } from "./fable_modules/fable-library.4.11.0/Array.js";

export class DefaultAnimation {
    constructor() {
    }
    Applies(_arg) {
        return true;
    }
    get IsDone() {
        return false;
    }
    AnimateHexagon(ctx, color, hexagons, hexagon) {
        let field;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        ctx.beginPath();
        let matchResult, field_1;
        if (GameStateModule_hoverField() != null) {
            if ((field = GameStateModule_hoverField(), field.Id === hexagon.FieldId)) {
                matchResult = 0;
                field_1 = GameStateModule_hoverField();
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
                const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 300);
                gradient.addColorStop(1, "white");
                gradient.addColorStop(0, color);
                ctx.fillStyle = gradient;
                break;
            }
            case 1: {
                ctx.fillStyle = color;
                break;
            }
        }
        ctx.lineWidth = 1;
        ctx.strokeStyle = color;
        ctx.globalAlpha = 1;
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
    AnimateOuterEdge(ctx, _arg, _arg_1, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 1;
            ctx.strokeStyle = "black";
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
    return class_type("Animations.DefaultAnimation", void 0, DefaultAnimation);
}

export function DefaultAnimation_$ctor() {
    return new DefaultAnimation();
}

export class SelectedAnimation {
    constructor() {
    }
    Applies(hexagon) {
        if (GameStateModule_selectedField() == null) {
            return false;
        }
        else {
            const field = GameStateModule_selectedField();
            return field.Id === hexagon.FieldId;
        }
    }
    get IsDone() {
        return false;
    }
    AnimateHexagon(ctx, color, _arg, hexagon) {
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        ctx.beginPath();
        ctx.fillStyle = color;
        ctx.lineWidth = 1;
        ctx.strokeStyle = color;
        ctx.globalAlpha = 1;
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
    AnimateOuterEdge(ctx, _arg, _arg_1, hexagon) {
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
    return class_type("Animations.SelectedAnimation", void 0, SelectedAnimation);
}

export function SelectedAnimation_$ctor() {
    return new SelectedAnimation();
}

export class CapturedAnimation {
    constructor(fieldId) {
        this.fieldId = fieldId;
        this.progress = 0;
        this.max = 100;
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
    AnimateHexagon(ctx, color, hexagons, hexagon) {
        const this$ = this;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 100);
        gradient.addColorStop(1 - (this$.progress / this$.max), color);
        gradient.addColorStop(0, "lime");
        ctx.beginPath();
        ctx.fillStyle = gradient;
        ctx.lineWidth = 0.2;
        ctx.strokeStyle = gradient;
        ctx.globalAlpha = 1;
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
    AnimateOuterEdge(ctx, color, hexagons, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 1;
            ctx.strokeStyle = "green";
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
    return class_type("Animations.CapturedAnimation", void 0, CapturedAnimation);
}

export function CapturedAnimation_$ctor_Z721C83C5(fieldId) {
    return new CapturedAnimation(fieldId);
}

export class FailedCaptureAnimation {
    constructor(fieldId) {
        this.fieldId = fieldId;
        this.progress = 0;
        this.max = 100;
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
    AnimateHexagon(ctx, color, hexagons, hexagon) {
        const this$ = this;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 100);
        gradient.addColorStop(0, color);
        gradient.addColorStop(this$.progress / this$.max, "red");
        ctx.beginPath();
        ctx.fillStyle = gradient;
        ctx.lineWidth = 0.4;
        ctx.strokeStyle = gradient;
        ctx.globalAlpha = 1;
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
    AnimateOuterEdge(ctx, color, hexagons, hexagon) {
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = 1;
            ctx.strokeStyle = "red";
            ctx.shadowBlur = 0;
            ctx.shadowColor = defaultOf();
            ctx.moveTo(p1.X, p1.Y);
            ctx.lineTo(p2.X, p2.Y);
            ctx.stroke();
            ctx.closePath();
        });
    }
}

export function FailedCaptureAnimation_$reflection() {
    return class_type("Animations.FailedCaptureAnimation", void 0, FailedCaptureAnimation);
}

export function FailedCaptureAnimation_$ctor_Z721C83C5(fieldId) {
    return new FailedCaptureAnimation(fieldId);
}

export class GainedDiceAnimation {
    constructor(fieldId, diceAdded) {
        this.fieldId = fieldId;
        this.diceAdded = (diceAdded | 0);
        this.progress = 0;
        this.max = 100;
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
    AnimateHexagon(ctx, color, hexagons, hexagon) {
        const this$ = this;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, (9 - this$.diceAdded) * 10);
        gradient.addColorStop(0, color);
        gradient.addColorStop(this$.progress / this$.max, "gold");
        ctx.beginPath();
        ctx.fillStyle = gradient;
        ctx.lineWidth = 0.4;
        ctx.strokeStyle = gradient;
        ctx.globalAlpha = 1;
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
    AnimateOuterEdge(ctx, color, hexagons, hexagon) {
        const this$ = this;
        hexagon.OuterEdges.forEach((tupledArg) => {
            const p1 = tupledArg[0];
            const p2 = tupledArg[1];
            ctx.beginPath();
            ctx.globalAlpha = 1;
            ctx.lineWidth = (this$.diceAdded * 0.2);
            ctx.strokeStyle = "white";
            ctx.shadowBlur = 0;
            ctx.shadowColor = defaultOf();
            ctx.moveTo(p1.X, p1.Y);
            ctx.lineTo(p2.X, p2.Y);
            ctx.stroke();
            ctx.closePath();
        });
    }
}

export function GainedDiceAnimation_$reflection() {
    return class_type("Animations.GainedDiceAnimation", void 0, GainedDiceAnimation);
}

export function GainedDiceAnimation_$ctor_Z18115A39(fieldId, diceAdded) {
    return new GainedDiceAnimation(fieldId, diceAdded);
}

export const defaultAnimation = DefaultAnimation_$ctor();

export const selectedAnimation = SelectedAnimation_$ctor();

export let runningAnimations = createAtom([]);

export function Animation_apply(method, ctx, color, hexagons, hexagon) {
    let array_1;
    const runningAnimation = tryFind((animation) => animation.Applies(hexagon), runningAnimations());
    if (runningAnimation == null) {
        if (selectedAnimation.Applies(hexagon)) {
            method(selectedAnimation, [ctx, color, hexagons, hexagon]);
        }
        else {
            method(defaultAnimation, [ctx, color, hexagons, hexagon]);
        }
    }
    else {
        const animation_1 = runningAnimation;
        method(animation_1, [ctx, color, hexagons, hexagon]);
    }
    if (runningAnimation != null) {
        runningAnimations((array_1 = runningAnimations(), array_1.filter((animation_2) => (animation_2.IsDone === false))));
    }
}

