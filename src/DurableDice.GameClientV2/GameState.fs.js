import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { union_type, record_type, int32_type, array_type, option_type, string_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { PlayerModule_isPlayer, PlayerModule_color, Player_$reflection } from "./Player.fs.js";
import { Field_$reflection } from "./Field.fs.js";
import { Position, CoordinateModule_toPosition, GameRules, GameRules_$reflection, Move_$reflection, Attack_$reflection } from "./Models.fs.js";
import { curry2, defaultOf, createAtom } from "./fable_modules/fable-library.4.11.0/Util.js";
import { HexMath_width } from "./HexMath.fs.js";
import { iterateIndexed, find } from "./fable_modules/fable-library.4.11.0/Array.js";

export class GameState extends Record {
    constructor(NextGameId, Players, Fields, ActivePlayerId, GameRound, PreviousAttack, PreviousMove, Rules) {
        super();
        this.NextGameId = NextGameId;
        this.Players = Players;
        this.Fields = Fields;
        this.ActivePlayerId = ActivePlayerId;
        this.GameRound = (GameRound | 0);
        this.PreviousAttack = PreviousAttack;
        this.PreviousMove = PreviousMove;
        this.Rules = Rules;
    }
}

export function GameState_$reflection() {
    return record_type("GameState.GameState", [], GameState, () => [["NextGameId", option_type(string_type)], ["Players", array_type(Player_$reflection())], ["Fields", array_type(Field_$reflection())], ["ActivePlayerId", option_type(string_type)], ["GameRound", int32_type], ["PreviousAttack", option_type(Attack_$reflection())], ["PreviousMove", option_type(Move_$reflection())], ["Rules", GameRules_$reflection()]]);
}

export class Response extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["GameState"];
    }
}

export function Response_$reflection() {
    return union_type("GameState.Response", [], Response, () => [[["Item", GameState_$reflection()]]]);
}

export class UIState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Uninitialized", "EnterName", "EnterRules", "WaitForAllReady", "WatchingOtherPlayers", "PlayerTurn", "MatchEnd", "Statistics"];
    }
}

export function UIState_$reflection() {
    return union_type("GameState.UIState", [], UIState, () => [[], [], [], [], [], [], [], []]);
}

export class RoundState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Idle", "FieldSelected"];
    }
}

export function RoundState_$reflection() {
    return union_type("GameState.RoundState", [], RoundState, () => [[], []]);
}

export let GameStateModule_currentUIState = createAtom(new UIState(0, []));

export let GameStateModule_currentRound = createAtom(0);

export let GameStateModule_selectedField = createAtom(void 0);

export let GameStateModule_hoverField = createAtom(void 0);

export let GameStateModule_currentRoundState = createAtom(new RoundState(0, []));

export let GameStateModule_currentState = createAtom(new GameState(void 0, [], [], void 0, 0, void 0, void 0, new GameRules(0, 0, 0, 0, 0)));

export function GameStateModule_resetRound() {
    GameStateModule_selectedField(void 0);
    GameStateModule_hoverField(void 0);
    GameStateModule_currentRoundState(new RoundState(0, []));
}

export function GameStateModule_drawPlayers(ctx, state) {
    let id;
    const boxWidth = 120;
    const playerCount = state.Players.length | 0;
    const totalWidth = playerCount * boxWidth;
    const offset = (HexMath_width - totalWidth) / 2;
    let i = 0;
    const arr = state.Players;
    for (let idx = 0; idx <= (arr.length - 1); idx++) {
        const player = arr[idx];
        const x = offset + (i * boxWidth);
        const isDead = player.ContinuousFieldCount === 0;
        let isActive;
        const matchValue = state.ActivePlayerId;
        let matchResult, id_1;
        if (matchValue != null) {
            if ((id = matchValue, id === player.Id)) {
                matchResult = 0;
                id_1 = matchValue;
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
                isActive = true;
                break;
            }
            default:
                isActive = false;
        }
        ctx.beginPath();
        if (isDead) {
            ctx.fillStyle = "silver";
            ctx.strokeStyle = "silver";
        }
        else {
            ctx.fillStyle = PlayerModule_color(player);
            ctx.strokeStyle = PlayerModule_color(player);
        }
        if (isActive && !isDead) {
            ctx.globalAlpha = 1;
            ctx.lineWidth = 2;
            ctx.strokeStyle = "white";
        }
        else {
            ctx.lineWidth = 0.4;
            ctx.globalAlpha = 0.75;
        }
        ctx.shadowBlur = 0;
        ctx.shadowColor = defaultOf();
        ctx.moveTo(x, 10);
        ctx.lineTo(x, 30);
        ctx.lineTo((x + boxWidth) - 3, 30);
        ctx.lineTo((x + boxWidth) - 3, 10);
        ctx.lineTo(x, 10);
        ctx.closePath();
        ctx.stroke();
        ctx.fill();
        if (isActive && !isDead) {
            ctx.fillStyle = "white";
        }
        else {
            ctx.fillStyle = "black";
        }
        ctx.font = "12px Verdana";
        ctx.textAlign = "center";
        ctx.fillText(`${player.Name} (${player.ContinuousFieldCount})`, (x + 3) + (boxWidth / 2), 24, boxWidth);
        i = ((i + 1) | 0);
    }
    return (value) => {
    };
}

export function GameStateModule_drawDice(ctx, state) {
    const arr = state.Fields;
    for (let idx = 0; idx <= (arr.length - 1); idx++) {
        const field = arr[idx];
        const position = CoordinateModule_toPosition(field.Center);
        ctx.fillStyle = "white";
        ctx.font = "12px Verdana";
        ctx.textAlign = "center";
        ctx.fillText(`${field.DiceCount} (${field.Index})`, position.X, position.Y + 4, 30);
    }
    return (value) => {
    };
}

export function GameStateModule_drawTurn(ctx, state) {
    const matchValue = state.PreviousAttack;
    const matchValue_1 = state.PreviousMove;
    let matchResult, attack, move;
    if (matchValue == null) {
        if (matchValue_1 != null) {
            matchResult = 1;
            move = matchValue_1;
        }
        else {
            matchResult = 2;
        }
    }
    else if (matchValue_1 == null) {
        matchResult = 0;
        attack = matchValue;
    }
    else {
        matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            const attackingPlayer = find((player) => PlayerModule_isPlayer(attack.AttackerId, player), state.Players);
            const defendingPlayer = find((player_1) => PlayerModule_isPlayer(attack.DefenderId, player_1), state.Players);
            const attackingColor = PlayerModule_color(attackingPlayer);
            const defendingColor = PlayerModule_color(defendingPlayer);
            const dicePositions = [[0, 0], [1, 0], [2, 0], [3, 0], [0, 1], [2, 1], [2, 1], [3, 1]];
            const pipPositions = [[0, 0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 1, 0, 0, 0, 0], [1, 0, 0, 0, 0, 0, 0, 0, 1], [1, 0, 0, 0, 1, 0, 0, 0, 1], [1, 0, 1, 0, 0, 0, 1, 0, 1], [1, 0, 1, 0, 1, 0, 1, 0, 1], [1, 0, 1, 1, 0, 1, 1, 0, 1]];
            const attackPosition = new Position(400, 1000);
            const defendPosition = new Position(600, 1000);
            const dieSize = 30;
            const drawDie = (color, pos) => {
                ctx.beginPath();
                ctx.fillStyle = color;
                ctx.strokeStyle = "black";
                ctx.lineWidth = 2;
                const x = pos.X;
                const y = pos.Y;
                ctx.moveTo(x, y);
                ctx.lineTo(x, y);
                ctx.lineTo(x + dieSize, y);
                ctx.lineTo(x + dieSize, y + dieSize);
                ctx.lineTo(x, y + dieSize);
                ctx.closePath();
                ctx.stroke();
                ctx.fill();
            };
            const drawAttackDie = curry2(drawDie)(attackingColor);
            const drawDefendDie = curry2(drawDie)(defendingColor);
            const drawPip = (delta, pos_1) => {
                ctx.beginPath();
                ctx.fillStyle = "black";
                ctx.strokeStyle = "black";
                ctx.lineWidth = 2;
                const x_1 = pos_1.X + delta.X;
                const y_1 = pos_1.Y + delta.Y;
                ctx.moveTo(x_1, y_1);
                ctx.arc(x_1, y_1, 2, 0, 3.141592653589793 * 2);
                ctx.closePath();
                ctx.stroke();
                ctx.fill();
            };
            const drawTlPip = curry2(drawPip)(new Position(5, 5));
            const drawTrPip = curry2(drawPip)(new Position(25, 5));
            const drawLPip = curry2(drawPip)(new Position(5, 25));
            const drawCPip = curry2(drawPip)(new Position(15, 15));
            const drawRPip = curry2(drawPip)(new Position(25, 25));
            const drawBlPip = curry2(drawPip)(new Position(5, 25));
            const drawBrPip = curry2(drawPip)(new Position(25, 25));
            const tl = (tupledArg) => {
                const x_2 = tupledArg[0];
                return x_2;
            };
            const tr = (tupledArg_1) => {
                const x_3 = tupledArg_1[2];
                return x_3;
            };
            const l = (tupledArg_2) => {
                const x_4 = tupledArg_2[3];
                return x_4;
            };
            const c = (tupledArg_3) => {
                const x_5 = tupledArg_3[4];
                return x_5;
            };
            const r = (tupledArg_4) => {
                const x_6 = tupledArg_4[5];
                return x_6;
            };
            const bl = (tupledArg_5) => {
                const x_7 = tupledArg_5[6];
                return x_7;
            };
            const br = (tupledArg_6) => {
                const x_8 = tupledArg_6[8];
                return x_8;
            };
            const drawPips = (pos_2, pips) => {
                if (tl(pips) === 1) {
                    drawTlPip(pos_2);
                }
                if (tr(pips) === 1) {
                    drawTrPip(pos_2);
                }
                if (l(pips) === 1) {
                    drawLPip(pos_2);
                }
                if (c(pips) === 1) {
                    drawCPip(pos_2);
                }
                if (r(pips) === 1) {
                    drawRPip(pos_2);
                }
                if (bl(pips) === 1) {
                    drawBlPip(pos_2);
                }
                if (br(pips) === 1) {
                    drawBrPip(pos_2);
                }
            };
            iterateIndexed((index, count) => {
                const position = dicePositions[index];
                const pos_3 = new Position(attackPosition.X - (position[0] * 40), attackPosition.Y - (position[1] * 40));
                drawAttackDie(pos_3);
                drawPips(pos_3, pipPositions[count]);
            }, attack.AttackingDiceCount);
            iterateIndexed((index_1, count_1) => {
                const position_1 = dicePositions[index_1];
                const pos_4 = new Position(defendPosition.X + (position_1[0] * 40), defendPosition.Y + (position_1[1] * 40));
                drawDefendDie(pos_4);
                drawPips(pos_4, pipPositions[count_1]);
            }, attack.DefendingDiceCount);
            break;
        }
    }
    return (value) => {
    };
}

