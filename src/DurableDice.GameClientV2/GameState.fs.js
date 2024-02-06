import { Record, Union } from "./fable_modules/fable-library.4.11.0/Types.js";
import { float64_type, array_type, record_type, option_type, bool_type, string_type, int32_type, union_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { Field_$reflection } from "./Field.fs.js";
import { createAtom } from "./fable_modules/fable-library.4.11.0/Util.js";

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
    return union_type("GameState.BotType", [], BotType, () => [[], [], []]);
}

export class Player extends Record {
    constructor(Index, Id, Name, DiceBuffer, DiceMovesThisTurn, ContinuousFieldCount, IsReady, BotType) {
        super();
        this.Index = (Index | 0);
        this.Id = Id;
        this.Name = Name;
        this.DiceBuffer = (DiceBuffer | 0);
        this.DiceMovesThisTurn = (DiceMovesThisTurn | 0);
        this.ContinuousFieldCount = (ContinuousFieldCount | 0);
        this.IsReady = IsReady;
        this.BotType = BotType;
    }
}

export function Player_$reflection() {
    return record_type("GameState.Player", [], Player, () => [["Index", int32_type], ["Id", string_type], ["Name", string_type], ["DiceBuffer", int32_type], ["DiceMovesThisTurn", int32_type], ["ContinuousFieldCount", int32_type], ["IsReady", bool_type], ["BotType", option_type(int32_type)]]);
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
    return record_type("GameState.Attack", [], Attack, () => [["AttackerId", string_type], ["AttackingFieldId", string_type], ["AttackingDiceCount", array_type(int32_type)], ["DefenderId", string_type], ["DefendingFieldId", string_type], ["DefendingDiceCount", array_type(int32_type)], ["IsSuccessful", bool_type]]);
}

export class Move extends Record {
    constructor(Count, AddedFieldId) {
        super();
        this.Count = (Count | 0);
        this.AddedFieldId = AddedFieldId;
    }
}

export function Move_$reflection() {
    return record_type("GameState.Move", [], Move, () => [["Count", int32_type], ["AddedFieldId", string_type]]);
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
    return record_type("GameState.GameRules", [], GameRules, () => [["StartDiceCountPerField", int32_type], ["InitialDiceBuffer", int32_type], ["MaxDiceMovedPerTurn", int32_type], ["DiceGenerationMultiplier", float64_type], ["DeadPlayerMultiplier", float64_type]]);
}

export class GameState extends Record {
    constructor(Fields) {
        super();
        this.Fields = Fields;
    }
}

export function GameState_$reflection() {
    return record_type("GameState.GameState", [], GameState, () => [["Fields", array_type(Field_$reflection())]]);
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
    return union_type("GameState.Action", [], Action, () => [[], [["PlayerId", string_type], ["BotType", BotType_$reflection()]], [["PlayerId", string_type], ["PlayerName", string_type]], [["PlayerId", string_type]], [["PlayerId", string_type], ["FromFieldId", string_type], ["ToFieldId", string_type]], [["PlayerId", string_type]], [["PlayerId", string_type]], [["PlayerId", string_type]]]);
}

export let selectedField = createAtom(void 0);

export let currentState = createAtom(new GameState([]));

