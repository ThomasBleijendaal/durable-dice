import { Union, Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { bool_type, array_type, string_type, union_type, float64_type, record_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { HexMath_rx, HexMath_paddingTop, HexMath_r, HexMath_r2, HexMath_paddingLeft } from "./HexMath.fs.js";
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

export function CoordinateModule_isSame(c1, c2) {
    if (c1.X === c2.X) {
        return c1.Y === c2.Y;
    }
    else {
        return false;
    }
}

export function CoordinateModule_toPosition(c) {
    return new Position(HexMath_paddingLeft + ((c.X * HexMath_r2) - ((c.Y % 2) * HexMath_r)), HexMath_paddingTop + (c.Y * (HexMath_r2 - (HexMath_r - HexMath_rx))));
}

export function CoordinateModule_neighbors(c) {
    if ((c.Y % 2) === 1) {
        return [[new EdgeType(0, []), new Coordinate(c.X - 1, c.Y - 1)], [new EdgeType(1, []), new Coordinate(c.X - 1, c.Y)], [new EdgeType(2, []), new Coordinate(c.X - 1, c.Y + 1)], [new EdgeType(3, []), new Coordinate(c.X, c.Y + 1)], [new EdgeType(4, []), new Coordinate(c.X + 1, c.Y)], [new EdgeType(5, []), new Coordinate(c.X, c.Y - 1)]];
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

export class Attack extends Record {
    constructor(AttackerId, AttackingFieldId, AttackingDiceCount, DefenderId, DefendingFieldId, DefendingDiceCount, IsSuccessful) {
        super();
        this.AttackerId = AttackerId;
        this.AttackingFieldId = AttackingFieldId;
        this.AttackingDiceCount = AttackingDiceCount;
        this.DefenderId = DefenderId;
        this.DefendingFieldId = DefendingFieldId;
        this.DefendingDiceCount = DefendingDiceCount;
        this.IsSuccessful = IsSuccessful;
    }
}

export function Attack_$reflection() {
    return record_type("Models.Attack", [], Attack, () => [["AttackerId", string_type], ["AttackingFieldId", string_type], ["AttackingDiceCount", array_type(int32_type)], ["DefenderId", string_type], ["DefendingFieldId", string_type], ["DefendingDiceCount", array_type(int32_type)], ["IsSuccessful", bool_type]]);
}

export class Move extends Record {
    constructor(Count, AddedFieldId) {
        super();
        this.Count = (Count | 0);
        this.AddedFieldId = AddedFieldId;
    }
}

export function Move_$reflection() {
    return record_type("Models.Move", [], Move, () => [["Count", int32_type], ["AddedFieldId", string_type]]);
}

export class GameRules extends Record {
    constructor(StartDiceCountPerField, InitialDiceBuffer, MaxDiceMovedPerTurn, DiceGenerationMultiplier, DeadPlayerMultiplier) {
        super();
        this.StartDiceCountPerField = (StartDiceCountPerField | 0);
        this.InitialDiceBuffer = (InitialDiceBuffer | 0);
        this.MaxDiceMovedPerTurn = (MaxDiceMovedPerTurn | 0);
        this.DiceGenerationMultiplier = DiceGenerationMultiplier;
        this.DeadPlayerMultiplier = DeadPlayerMultiplier;
    }
}

export function GameRules_$reflection() {
    return record_type("Models.GameRules", [], GameRules, () => [["StartDiceCountPerField", int32_type], ["InitialDiceBuffer", int32_type], ["MaxDiceMovedPerTurn", int32_type], ["DiceGenerationMultiplier", float64_type], ["DeadPlayerMultiplier", float64_type]]);
}

export class BotType extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["CheezyBot", "StrategicBot", "NerdBot"];
    }
}

export function BotType_$reflection() {
    return union_type("Models.BotType", [], BotType, () => [[], [], []]);
}

export class AddBotCommand extends Record {
    constructor(BotType) {
        super();
        this.BotType = BotType;
    }
}

export function AddBotCommand_$reflection() {
    return record_type("Models.AddBotCommand", [], AddBotCommand, () => [["BotType", BotType_$reflection()]]);
}

export class AddPlayerCommand extends Record {
    constructor(PlayerName) {
        super();
        this.PlayerName = PlayerName;
    }
}

export function AddPlayerCommand_$reflection() {
    return record_type("Models.AddPlayerCommand", [], AddPlayerCommand, () => [["PlayerName", string_type]]);
}

export class MoveFieldCommand extends Record {
    constructor(FromFieldId, ToFieldId) {
        super();
        this.FromFieldId = FromFieldId;
        this.ToFieldId = ToFieldId;
    }
}

export function MoveFieldCommand_$reflection() {
    return record_type("Models.MoveFieldCommand", [], MoveFieldCommand, () => [["FromFieldId", string_type], ["ToFieldId", string_type]]);
}

export class ReadyWithRulesCommand extends Record {
    constructor(GameRules) {
        super();
        this.GameRules = GameRules;
    }
}

export function ReadyWithRulesCommand_$reflection() {
    return record_type("Models.ReadyWithRulesCommand", [], ReadyWithRulesCommand, () => [["GameRules", GameRules_$reflection()]]);
}

export class Action extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["JoinGame", "AddBot", "AddPlayer", "RemovePlayer", "MoveField", "EndRound", "Ready", "ReadyWithRules"];
    }
}

export function Action_$reflection() {
    return union_type("Models.Action", [], Action, () => [[], [["Item", AddBotCommand_$reflection()]], [["Item", AddPlayerCommand_$reflection()]], [], [["Item", MoveFieldCommand_$reflection()]], [], [], [["Item", ReadyWithRulesCommand_$reflection()]]]);
}

