import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { record_type, option_type, bool_type, string_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { color } from "./Theme.fs.js";

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
    return record_type("Player.Player", [], Player, () => [["Index", int32_type], ["Id", string_type], ["Name", string_type], ["DiceBuffer", int32_type], ["DiceMovesThisTurn", int32_type], ["ContinuousFieldCount", int32_type], ["IsReady", bool_type], ["BotType", option_type(int32_type)]]);
}

export function PlayerModule_isPlayer(playerId, player) {
    return player.Id === playerId;
}

export function PlayerModule_isDead(player) {
    return player.ContinuousFieldCount === 0;
}

export function PlayerModule_color(player) {
    return color(player.Index);
}

