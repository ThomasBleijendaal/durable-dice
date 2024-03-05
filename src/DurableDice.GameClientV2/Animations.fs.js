import { GameStateModule_selectedField, GameStateModule_targetField, GameStateModule_hoverField } from "./GameState.fs.js";
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
        let field, field_1, field_2;
        const edges = hexagon.AllEdges;
        const firstEdge = edges[0][0];
        ctx.beginPath();
        const matchValue = GameStateModule_hoverField();
        const matchValue_1 = GameStateModule_targetField();
        let matchResult, field_3, field_4;
        if (matchValue_1 != null) {
            if ((field = matchValue_1, field.Id === hexagon.FieldId)) {
                matchResult = 0;
                field_3 = matchValue_1;
            }
            else if (matchValue != null) {
                if ((field_1 = matchValue, field_1.Id === hexagon.FieldId)) {
                    matchResult = 1;
                    field_4 = matchValue;
                }
                else {
                    matchResult = 2;
                }
            }
            else {
                matchResult = 2;
            }
        }
        else if (matchValue != null) {
            if ((field_2 = matchValue, field_2.Id === hexagon.FieldId)) {
                matchResult = 1;
                field_4 = matchValue;
            }
            else {
                matchResult = 2;
            }
        }
        else {
            matchResult = 2;
        }
        switch (matchResult) {
            case 0: {
                const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 100);
                gradient.addColorStop(1, "white");
                gradient.addColorStop(0, color);
                ctx.fillStyle = gradient;
                break;
            }
            case 1: {
                const gradient_1 = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 200);
                gradient_1.addColorStop(1, "white");
                gradient_1.addColorStop(0, color);
                ctx.fillStyle = gradient_1;
                break;
            }
            case 2: {
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
    constructor(fromFieldId, toFieldId) {
        this.fromFieldId = fromFieldId;
        this.toFieldId = toFieldId;
        this.progress = 0;
        this.max = 12;
    }
    Applies(hexagon) {
        const this$ = this;
        return (hexagon.FieldId === this$.fromFieldId) ? true : (hexagon.FieldId === this$.toFieldId);
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
        const gradient = ctx.createRadialGradient(hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 0, hexagons.CenterPosition.X, hexagons.CenterPosition.Y, 300);
        const patternInput = (hexagon.FieldId === this$.toFieldId) ? [0, this$.progress / this$.max] : [0.4 - ((this$.progress / this$.max) / 3), 0];
        const stop = patternInput[1];
        const start = patternInput[0];
        gradient.addColorStop(start, color);
        gradient.addColorStop(stop, "lime");
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

export function CapturedAnimation_$ctor_Z384F8060(fromFieldId, toFieldId) {
    return new CapturedAnimation(fromFieldId, toFieldId);
}

export class FailedCaptureAnimation {
    constructor(fieldId) {
        this.fieldId = fieldId;
        this.progress = 0;
        this.max = 12;
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
        gradient.addColorStop((this$.progress / this$.max) / 3, "red");
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
        this.max = 12;
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
        const progressPart = (this$.progress / this$.max) / 4;
        gradient.addColorStop(0, color);
        gradient.addColorStop(progressPart, color);
        gradient.addColorStop(progressPart + 0.1, "white");
        gradient.addColorStop(progressPart + 0.2, color);
        gradient.addColorStop(progressPart + 0.3, "white");
        gradient.addColorStop(progressPart + 0.4, color);
        gradient.addColorStop(progressPart + 0.5, "white");
        gradient.addColorStop(progressPart + 0.6, color);
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

export function GainedDiceAnimation_$reflection() {
    return class_type("Animations.GainedDiceAnimation", void 0, GainedDiceAnimation);
}

export function GainedDiceAnimation_$ctor_Z18115A39(fieldId, diceAdded) {
    return new GainedDiceAnimation(fieldId, diceAdded);
}

export const defaultAnimation = DefaultAnimation_$ctor();

export const selectedAnimation = SelectedAnimation_$ctor();

export let runningAnimations = createAtom([]);

export function Animation_completeDone() {
    let array;
    if (runningAnimations().length > 0) {
        runningAnimations((array = runningAnimations(), array.filter((animation) => (animation.IsDone === false))));
    }
}

export function Animation_apply(method, ctx, color, hexagons, hexagon) {
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
}

