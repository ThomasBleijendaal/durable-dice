import { newGuid, tryParse } from "./fable_modules/fable-library.4.11.0/Guid.js";
import { FSharpRef } from "./fable_modules/fable-library.4.11.0/Types.js";
import { substring, replace } from "./fable_modules/fable-library.4.11.0/String.js";
import { equals as equals_1, uncurry2, disposeSafe, getEnumerator, stringHash, numberHash, comparePrimitives, createAtom } from "./fable_modules/fable-library.4.11.0/Util.js";
import { FSharpMap__get_Item, ofSeq } from "./fable_modules/fable-library.4.11.0/Map.js";
import { name, getUnionFields } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { Action, MoveFieldCommand, Position, Action_$reflection } from "./Models.fs.js";
import { HexagonModule_isInside } from "./Hexagon.fs.js";
import { collect, map, append, find, contains, tryHead, tryFind } from "./fable_modules/fable-library.4.11.0/Array.js";
import { GameStateModule_drawTurn, GameStateModule_drawDice, GameStateModule_drawPlayers, GameStateModule_currentRound, GameStateModule_currentActionCount, GameStateModule_resetRound, UIState, GameStateModule_currentUIState, GameStateModule_hoverField, RoundState, GameStateModule_targetField, GameStateModule_currentRoundState, GameStateModule_selectedField, GameStateModule_currentState } from "./GameState.fs.js";
import { PlayerModule_isPlayer } from "./Player.fs.js";
import { Array_groupBy } from "./fable_modules/fable-library.4.11.0/Seq2.js";
import { equals } from "./fable_modules/fable-library.4.11.0/BigInt.js";
import { Animation_completeDone, Animation_apply, GainedDiceAnimation_$ctor_Z18115A39, runningAnimations, FailedCaptureAnimation_$ctor_Z721C83C5, CapturedAnimation_$ctor_Z384F8060 } from "./Animations.fs.js";
import { FieldModule_groupHexagons } from "./Field.fs.js";
import { color as color_1 } from "./Theme.fs.js";
import { some } from "./fable_modules/fable-library.4.11.0/Option.js";
import { sortBy } from "./fable_modules/fable-library.4.11.0/Seq.js";

export const endpoint = "http://localhost:7071";

export const playerId = (() => {
    let matchValue;
    let outArg = "00000000-0000-0000-0000-000000000000";
    matchValue = [tryParse(localStorage.getItem("player-id"), new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        const value = matchValue[1];
        return value;
    }
    else {
        let copyOfStruct = newGuid();
        return copyOfStruct;
    }
})();

localStorage.setItem("player-id", playerId);

export const gameIdFromUrl = replace(window.location.pathname, "/", "");

export function createGameId() {
    let copyOfStruct;
    return substring((copyOfStruct = newGuid(), copyOfStruct), 0, 6);
}

export const gameId = (() => {
    let url, storage, url_1;
    const matchValue = localStorage.getItem("game-id");
    if ((url = gameIdFromUrl, (storage = matchValue, (url === storage) && (url.length === 6)))) {
        const url_2 = gameIdFromUrl;
        const storage_1 = matchValue;
        return storage_1;
    }
    else if ((url_1 = gameIdFromUrl, url_1.length === 6)) {
        const url_3 = gameIdFromUrl;
        return url_3;
    }
    else {
        return createGameId();
    }
})();

if (gameIdFromUrl !== gameId) {
    history.pushState(void 0, document.title, `${window.location.protocol}//${window.location.host}/${gameId}`);
}

localStorage.setItem("game-id", gameId);

document.getElementById("shareurl").innerHTML = window.location.href;

export let currentFieldHexagons = createAtom([]);

export let allHexagons = createAtom([]);

export let ownerColors = createAtom(ofSeq([], {
    Compare: comparePrimitives,
}));

export const canvas = document.getElementById("canvas");

export let signalRCommand = createAtom((_arg) => {
});

export function sendAction(action) {
    const matchValue = getUnionFields(action, Action_$reflection());
    const union = matchValue[0];
    const data = matchValue[1];
    signalRCommand()([name(union), data[0]]);
}

export function findHexagon(event) {
    const rect = canvas.getBoundingClientRect();
    const pos = new Position((event.x - rect.left) * 1, (event.y - rect.top) * 1);
    const isHover = (hex) => HexagonModule_isInside(pos, hex);
    return tryFind(isHover, allHexagons());
}

export function findField(id) {
    return tryFind((field) => (field.Id === id), GameStateModule_currentState().Fields);
}

export function isActivePlayer() {
    let id;
    const matchValue = GameStateModule_currentState().ActivePlayerId;
    let matchResult, id_1;
    if (matchValue != null) {
        if ((id = matchValue, id === playerId)) {
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
        case 0:
            return true;
        default:
            return false;
    }
}

export function isAdmin() {
    let player;
    const matchValue = tryHead(GameStateModule_currentState().Players);
    let matchResult, player_1;
    if (matchValue != null) {
        if ((player = matchValue, player.Id === playerId)) {
            matchResult = 0;
            player_1 = matchValue;
        }
        else {
            matchResult = 1;
        }
    }
    else {
        matchResult = 1;
    }
    switch (matchResult) {
        case 0:
            return true;
        default:
            return false;
    }
}

export function isAlivePlayer() {
    let player_1;
    const matchValue = tryFind((player) => PlayerModule_isPlayer(playerId, player), GameStateModule_currentState().Players);
    let matchResult, player_2;
    if (matchValue != null) {
        if ((player_1 = matchValue, player_1.ContinuousFieldCount === 0)) {
            matchResult = 0;
            player_2 = matchValue;
        }
        else {
            matchResult = 1;
        }
    }
    else {
        matchResult = 1;
    }
    switch (matchResult) {
        case 0:
            return false;
        default:
            return true;
    }
}

export function handleClickOnHexagon(foundHexagon) {
    let field_2, field_3, field;
    const matchValue_1 = GameStateModule_selectedField();
    if (GameStateModule_currentRoundState().tag === 1) {
        if (foundHexagon != null) {
            if (matchValue_1 != null) {
                const attackingField = matchValue_1;
                const hexagon_1 = foundHexagon;
                const foundField_1 = findField(hexagon_1.FieldId);
                if (foundField_1 != null) {
                    if ((field_2 = foundField_1, ((field_2.OwnerId !== playerId) && (attackingField.DiceCount > 1)) && contains(field_2.Index, attackingField.Neighbors, {
                        Equals: (x, y) => (x === y),
                        GetHashCode: numberHash,
                    }))) {
                        const field_4 = foundField_1;
                        sendAction(new Action(4, [new MoveFieldCommand(attackingField.Id, field_4.Id)]));
                        GameStateModule_targetField(field_4);
                        GameStateModule_selectedField(void 0);
                        GameStateModule_currentRoundState(new RoundState(0, []));
                    }
                    else if ((field_3 = foundField_1, field_3.OwnerId === playerId)) {
                        const field_5 = foundField_1;
                        GameStateModule_selectedField(field_5);
                    }
                }
                else {
                    GameStateModule_selectedField(void 0);
                    GameStateModule_currentRoundState(new RoundState(0, []));
                }
            }
        }
        else {
            GameStateModule_selectedField(void 0);
            GameStateModule_currentRoundState(new RoundState(0, []));
        }
    }
    else if (foundHexagon != null) {
        const hexagon = foundHexagon;
        const foundField = findField(hexagon.FieldId);
        let matchResult, field_1;
        if (foundField != null) {
            if ((field = foundField, field.OwnerId === playerId)) {
                matchResult = 0;
                field_1 = foundField;
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
                GameStateModule_selectedField(findField(hexagon.FieldId));
                GameStateModule_currentRoundState(new RoundState(1, []));
                break;
            }
            case 1: {
                GameStateModule_selectedField(void 0);
                GameStateModule_currentRoundState(new RoundState(0, []));
                break;
            }
        }
    }
    else {
        GameStateModule_selectedField(void 0);
        GameStateModule_currentRoundState(new RoundState(0, []));
    }
}

export function handleMouseOnHexagon(foundHexagon) {
    let field, selectedField, field_2;
    let matchResult;
    if (GameStateModule_currentRoundState().tag === 1) {
        if (foundHexagon != null) {
            matchResult = 1;
        }
        else {
            matchResult = 2;
        }
    }
    else if (foundHexagon != null) {
        matchResult = 0;
    }
    else {
        matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            const hexagon = foundHexagon;
            const matchValue_2 = findField(hexagon.FieldId);
            let matchResult_1, field_1;
            if (matchValue_2 != null) {
                if ((field = matchValue_2, field.OwnerId === playerId)) {
                    matchResult_1 = 0;
                    field_1 = matchValue_2;
                }
                else {
                    matchResult_1 = 1;
                }
            }
            else {
                matchResult_1 = 1;
            }
            switch (matchResult_1) {
                case 0: {
                    GameStateModule_hoverField(field_1);
                    break;
                }
                case 1: {
                    GameStateModule_hoverField(void 0);
                    break;
                }
            }
            break;
        }
        case 1: {
            const hexagon_1 = foundHexagon;
            const matchValue_3 = findField(hexagon_1.FieldId);
            const matchValue_4 = GameStateModule_selectedField();
            let matchResult_2, field_3, selectedField_1;
            if (matchValue_3 != null) {
                if (matchValue_4 != null) {
                    if ((selectedField = matchValue_4, (field_2 = matchValue_3, contains(field_2.Index, selectedField.Neighbors, {
                        Equals: (x, y) => (x === y),
                        GetHashCode: numberHash,
                    })))) {
                        matchResult_2 = 0;
                        field_3 = matchValue_3;
                        selectedField_1 = matchValue_4;
                    }
                    else {
                        matchResult_2 = 1;
                    }
                }
                else {
                    matchResult_2 = 1;
                }
            }
            else {
                matchResult_2 = 1;
            }
            switch (matchResult_2) {
                case 0: {
                    GameStateModule_hoverField(field_3);
                    break;
                }
                case 1: {
                    GameStateModule_hoverField(void 0);
                    break;
                }
            }
            break;
        }
        case 2: {
            GameStateModule_hoverField(void 0);
            break;
        }
    }
}

export function mouseclick(event) {
    if (isActivePlayer()) {
        const foundHexagon = findHexagon(event);
        handleClickOnHexagon(foundHexagon);
    }
}

export function mousemove(event) {
    if (isActivePlayer()) {
        const foundHexagon = findHexagon(event);
        handleMouseOnHexagon(foundHexagon);
    }
}

canvas.onclick = ((event) => {
    mouseclick(event);
});

canvas.onmousemove = ((event) => {
    mousemove(event);
});

export function updateGameState(state) {
    let state_1, array_2, array_5, array_9, players;
    if (state != null) {
        if ((state_1 = state, state_1.Fields.length === 0)) {
            const state_2 = state;
            const player = find((p) => (p.Id === playerId), state_2.Players);
            if (player.IsReady === false) {
                GameStateModule_currentUIState(new UIState(2, []));
            }
            else {
                GameStateModule_currentUIState(new UIState(3, []));
            }
            GameStateModule_currentState(state_2);
        }
        else {
            const state_3 = state;
            GameStateModule_currentState(state_3);
            const nextUrl = document.getElementById("nexturl");
            nextUrl.href = (`${window.location.protocol}//${window.location.host}/${state_3.NextGameId}`);
            if (isActivePlayer() === false) {
                GameStateModule_resetRound();
                GameStateModule_currentUIState(new UIState(4, []));
            }
            else {
                GameStateModule_currentUIState(new UIState(5, []));
            }
            if (((array_2 = Array_groupBy((f) => f.OwnerId, GameStateModule_currentState().Fields, {
                Equals: (x, y) => (x === y),
                GetHashCode: stringHash,
            }), array_2.length)) === 1) {
                GameStateModule_currentUIState(new UIState(6, []));
            }
            if (!equals(GameStateModule_currentActionCount(), state_3.GameActionCount)) {
                GameStateModule_targetField(void 0);
                GameStateModule_selectedField(void 0);
                GameStateModule_currentActionCount(state_3.GameActionCount);
                const matchValue = GameStateModule_currentState().PreviousAttack;
                if (matchValue != null) {
                    const attack = matchValue;
                    const animation = attack.IsSuccessful ? CapturedAnimation_$ctor_Z384F8060(attack.AttackingFieldId, attack.DefendingFieldId) : FailedCaptureAnimation_$ctor_Z721C83C5(attack.AttackingFieldId);
                    runningAnimations(append([animation], runningAnimations()));
                }
                if (GameStateModule_currentRound() !== GameStateModule_currentState().GameRound) {
                    GameStateModule_currentRound(GameStateModule_currentState().GameRound);
                    runningAnimations(append(map((field_1) => GainedDiceAnimation_$ctor_Z18115A39(field_1.Id, field_1.DiceAdded), (array_5 = GameStateModule_currentState().Fields, array_5.filter((field) => (field.DiceAdded > 0)))), runningAnimations()));
                }
                if (currentFieldHexagons().length === 0) {
                    currentFieldHexagons((array_9 = GameStateModule_currentState().Fields, map((players = GameStateModule_currentState().Players, (field_2) => FieldModule_groupHexagons(players, field_2)), array_9)));
                    allHexagons(collect((x_2) => x_2, map((hexGroups) => collect((x_1) => x_1, hexGroups.Hexagons), currentFieldHexagons())));
                    ownerColors(ofSeq(map((player_1) => [player_1.Id, color_1(player_1.Index)], GameStateModule_currentState().Players), {
                        Compare: comparePrimitives,
                    }));
                }
            }
            else {
                console.log(some("Duplicate state received"));
            }
        }
    }
    else {
        GameStateModule_currentUIState(new UIState(1, []));
    }
}

export function registerCallback(callback) {
    signalRCommand(callback);
}

export function show(id) {
    document.getElementById(id).className = "show";
}

export function hide(id) {
    document.getElementById(id).className = "hide";
}

export let previousTime = createAtom(0);

export let previousUIState = createAtom(new UIState(0, []));

export function draw(time) {
    window.requestAnimationFrame((arg) => {
        const value = draw(arg);
    });
    const delta = time - previousTime();
    if (delta > 15) {
        previousTime(time);
        const w = canvas.width;
        const h = canvas.height;
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, w, h);
        let selectedFieldId;
        if (GameStateModule_selectedField() == null) {
            selectedFieldId = "";
        }
        else {
            const field = GameStateModule_selectedField();
            selectedFieldId = field.Id;
        }
        const enumerator = getEnumerator(sortBy((g) => (g.FieldId === selectedFieldId), currentFieldHexagons(), {
            Compare: comparePrimitives,
        }));
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const group = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                const apply = (method) => {
                    const arr = group.Hexagons;
                    for (let idx = 0; idx <= (arr.length - 1); idx++) {
                        const hexagons = arr[idx];
                        const field_2 = find((field_1) => (field_1.Id === group.FieldId), GameStateModule_currentState().Fields);
                        const color = FSharpMap__get_Item(ownerColors(), field_2.OwnerId);
                        for (let idx_1 = 0; idx_1 <= (hexagons.length - 1); idx_1++) {
                            const hexagon = hexagons[idx_1];
                            Animation_apply(uncurry2(method), ctx, color, group, hexagon);
                        }
                    }
                };
                apply((animation) => ((tupledArg) => {
                    animation.AnimateHexagon(tupledArg[0], tupledArg[1], tupledArg[2], tupledArg[3]);
                }));
                apply((animation_1) => ((tupledArg_1) => {
                    animation_1.AnimateOuterEdge(tupledArg_1[0], tupledArg_1[1], tupledArg_1[2], tupledArg_1[3]);
                }));
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        Animation_completeDone();
        GameStateModule_drawPlayers(ctx, GameStateModule_currentState());
        GameStateModule_drawDice(ctx, GameStateModule_currentState());
        GameStateModule_drawTurn(ctx, GameStateModule_currentState());
        if (!equals_1(previousUIState(), GameStateModule_currentUIState())) {
            if (GameStateModule_currentUIState().tag === 1) {
                hide("ready");
                hide("end");
                show("form");
                show("joinGame");
                hide("rules");
                hide("gameRules");
                hide("dead");
                hide("winner");
            }
            else if (GameStateModule_currentUIState().tag === 2) {
                show("ready");
                hide("end");
                show("form");
                hide("joinGame");
                show("rules");
                if (isAdmin()) {
                    show("gameRules");
                }
                else {
                    hide("gameRules");
                }
                hide("dead");
                hide("winner");
            }
            else if (GameStateModule_currentUIState().tag === 3) {
                hide("ready");
                hide("end");
                hide("form");
                hide("joinGame");
                hide("rules");
                hide("gameRules");
                hide("dead");
                hide("winner");
            }
            else if (GameStateModule_currentUIState().tag === 4) {
                hide("ready");
                hide("end");
                hide("form");
                hide("joinGame");
                hide("rules");
                hide("gameRules");
                if (isAlivePlayer()) {
                    hide("dead");
                }
                else {
                    show("dead");
                }
                hide("winner");
            }
            else if (GameStateModule_currentUIState().tag === 5) {
                hide("ready");
                show("end");
                hide("form");
                hide("joinGame");
                hide("rules");
                hide("gameRules");
                hide("dead");
                hide("winner");
            }
            else if (GameStateModule_currentUIState().tag === 6) {
                hide("ready");
                hide("end");
                hide("form");
                hide("joinGame");
                hide("rules");
                hide("gameRules");
                if (isAlivePlayer()) {
                    hide("dead");
                    show("winner");
                }
                else {
                    show("dead");
                    hide("winner");
                }
            }
            previousUIState(GameStateModule_currentUIState());
        }
    }
}

(function () {
    const value = draw(0);
})();

updateGameState(void 0);

