import { newGuid } from "./fable_modules/fable-library.4.11.0/Guid.js";
import { uncurry2, disposeSafe, getEnumerator, equals, comparePrimitives, createAtom } from "./fable_modules/fable-library.4.11.0/Util.js";
import { choose, addRangeInPlace, tryFind } from "./fable_modules/fable-library.4.11.0/Array.js";
import { BotType, Action, Action_$reflection, GameState_$reflection, selectedField, currentState } from "./GameState.fs.js";
import { ofSeq } from "./fable_modules/fable-library.4.11.0/Map.js";
import { HubConnectionBuilder$5__withUrl_Z6B680FF3, HubConnectionBuilder$5__withAutomaticReconnect, HubConnectionBuilder$5__configureLogging_2D37BB17, HubConnectionBuilder$5__onMessage_2163CAFC } from "./fable_modules/Fable.SignalR.0.11.5/SignalR.fs.js";
import { toFail, toText, printf, toConsole } from "./fable_modules/fable-library.4.11.0/String.js";
import { ConnectionBuilder__header_Z305ADAA2 } from "./fable_modules/Fable.SignalR.0.11.5/HttpClient.fs.js";
import { HubConnectionBuilder$5_$ctor_Z66CB2AA1 } from "./fable_modules/Fable.SignalR.0.11.5/./SignalR.fs.js";
import { HubConnection$5_$ctor_3ED56BCC, Bindings_signalR } from "./fable_modules/Fable.SignalR.0.11.5/./HubConnection.fs.js";
import { Json_TextMessageFormat_write, Json_TextMessageFormat_parse, Json_JsonProtocol_$ctor, MsgPack_parseMsg, MsgPack_MsgPackProtocol_$ctor } from "./fable_modules/Fable.SignalR.0.11.5/./Protocols.fs.js";
import { singleton, reverse, cons, empty } from "./fable_modules/fable-library.4.11.0/List.js";
import { Reader__Read_24524716, Reader_$ctor_Z3F6BC7B1 } from "./fable_modules/Fable.SignalR.0.11.5/../Fable.Remoting.MsgPack.1.13.0/Read.fs.js";
import { fromInt32, op_Subtraction, compare, op_Addition, toUInt64 } from "./fable_modules/fable-library.4.11.0/BigInt.js";
import { class_type, enum_type, int32_type, unit_type, uint64_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { InvokeArg$1_$reflection, MsgPack_Msg$4, MsgPack_Msg$4_$reflection } from "./fable_modules/Fable.SignalR.0.11.5/Shared.fs.js";
import { writeObject } from "./fable_modules/Fable.SignalR.0.11.5/../Fable.Remoting.MsgPack.1.13.0/Write.fs.js";
import { SimpleJson_readPath, SimpleJson_parse } from "./fable_modules/Fable.SignalR.0.11.5/../Fable.SimpleJson.3.21.0/SimpleJson.fs.js";
import { map, value as value_9 } from "./fable_modules/fable-library.4.11.0/Option.js";
import { createTypeInfo } from "./fable_modules/Fable.SimpleJson.3.21.0/./TypeInfo.Converter.fs.js";
import { Convert_serialize, Convert_fromJson } from "./fable_modules/Fable.SimpleJson.3.21.0/./Json.Converter.fs.js";
import { Result_Map, FSharpResult$2 } from "./fable_modules/fable-library.4.11.0/Choice.js";
import { HubRecords_CloseMessage_$reflection, HubRecords_PingMessage_$reflection, HubRecords_CancelInvocationMessage_$reflection, HubRecords_StreamInvocationMessage$1_$reflection, HubRecords_CompletionMessage$1_$reflection, HubRecords_StreamItemMessage$1_$reflection, HubRecords_InvocationMessage$1_$reflection } from "./fable_modules/Fable.SignalR.0.11.5/Protocols.fs.js";
import { HubConnection$5__sendAsPromise_2B595, HubConnection$5__sendNow_2B595, HubConnection$5__startNow } from "./fable_modules/Fable.SignalR.0.11.5/HubConnection.fs.js";
import { Position } from "./Models.fs.js";
import { HexagonModule_isInside } from "./Hexagon.fs.js";
import { PromiseBuilder__Delay_62FBFDE1, PromiseBuilder__Run_212F1D4B } from "./fable_modules/Fable.Promise.2.2.2/Promise.fs.js";
import { promise } from "./fable_modules/Fable.Promise.2.2.2/PromiseImpl.fs.js";
import { sortBy } from "./fable_modules/fable-library.4.11.0/Seq.js";
import { Animation_apply } from "./Animations.fs.js";

export const endpoint = "http://localhost:7071";

export const gameId = (() => {
    let copyOfStruct = newGuid();
    return copyOfStruct;
})();

export const playerId = (() => {
    let copyOfStruct = newGuid();
    return copyOfStruct;
})();

export let currentFieldHexagons = createAtom([]);

export let allHexagons = createAtom([]);

export const canvas = document.getElementById("canvas");

export function selectField(foundHexagon) {
    let hex, field_1;
    const findField = (id) => tryFind((field) => (field.Id === id), currentState().Fields);
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

export const headers = ofSeq([["x-playerid", playerId], ["x-game", gameId]], {
    Compare: comparePrimitives,
});

export const signalRHub = (() => {
    let protocol_2, protocol, protocol_1;
    const _ = HubConnectionBuilder$5__onMessage_2163CAFC(HubConnectionBuilder$5__configureLogging_2D37BB17(HubConnectionBuilder$5__withAutomaticReconnect(HubConnectionBuilder$5__withUrl_Z6B680FF3(HubConnectionBuilder$5_$ctor_Z66CB2AA1(new Bindings_signalR.HubConnectionBuilder()), endpoint, (options) => ConnectionBuilder__header_Z305ADAA2(options, headers))), 1), (response) => {
        toConsole(printf("PAAAARSE"));
    });
    return HubConnection$5_$ctor_3ED56BCC((protocol_2 = (_.useMsgPack ? ((protocol = MsgPack_MsgPackProtocol_$ctor(), {
        name: "messagepack",
        version: 1,
        transferFormat: 2,
        parseMessages(input, logger) {
            let collection;
            try {
                const buffer_2 = input;
                const reader = Reader_$ctor_Z3F6BC7B1(new Uint8Array(buffer_2));
                const read = (pos_mut, xs_mut) => {
                    let pos_1;
                    read:
                    while (true) {
                        const pos = pos_mut, xs = xs_mut;
                        const matchValue = toUInt64(op_Addition(toUInt64(op_Addition(Reader__Read_24524716(reader, uint64_type), pos)), 1n));
                        if ((pos_1 = matchValue, compare(toUInt64(op_Subtraction(toUInt64(fromInt32(buffer_2.byteLength)), pos_1)), 0n) > 0)) {
                            const pos_2 = matchValue;
                            pos_mut = pos_2;
                            xs_mut = cons(MsgPack_parseMsg(Reader__Read_24524716(reader, MsgPack_Msg$4_$reflection(unit_type, GameState_$reflection(), GameState_$reflection(), unit_type))), xs);
                            continue read;
                        }
                        else {
                            return cons(MsgPack_parseMsg(Reader__Read_24524716(reader, MsgPack_Msg$4_$reflection(unit_type, GameState_$reflection(), GameState_$reflection(), unit_type))), xs);
                        }
                        break;
                    }
                };
                collection = reverse(read(0n, empty()));
            }
            catch (e) {
                let message;
                const arg = e.message;
                message = toText(printf("An error occured during message deserialization: %s"))(arg);
                logger.log(4, message);
                collection = empty();
            }
            return Array.from(collection);
        },
        writeMessage(msg_2) {
            let matchValue_1, invocation, matchValue_1_1, invocation_1, arg_1, streamItem, completion, streamInvocation, cancelInvocation, close;
            const message_1 = msg_2;
            const outArr = [];
            writeObject((matchValue_1 = (message_1.type | 0), (matchValue_1 === 1) ? ((invocation = message_1, (matchValue_1_1 = invocation.target, (matchValue_1_1 === "Invoke") ? ((invocation.arguments.length === 2) ? (new MsgPack_Msg$4(1, [invocation.headers, invocation.invocationId, invocation.target, invocation.arguments[0], invocation.arguments[1], invocation.streamIds])) : ((invocation_1 = message_1, new MsgPack_Msg$4(2, [invocation_1.headers, invocation_1.invocationId, invocation_1.target, invocation_1.arguments, invocation_1.streamIds])))) : ((matchValue_1_1 === "Send") ? (new MsgPack_Msg$4(0, [invocation.headers, invocation.invocationId, invocation.target, invocation.arguments, invocation.streamIds])) : ((matchValue_1_1 === "StreamTo") ? (new MsgPack_Msg$4(0, [invocation.headers, invocation.invocationId, invocation.target, invocation.arguments, invocation.streamIds])) : ((arg_1 = invocation.target, toFail(printf("Invalid Invocation Target: %s"))(arg_1)))))))) : ((matchValue_1 === 2) ? ((streamItem = message_1, new MsgPack_Msg$4(3, [streamItem.headers, streamItem.invocationId, streamItem.item]))) : ((matchValue_1 === 3) ? ((completion = message_1, new MsgPack_Msg$4(4, [completion.headers, completion.invocationId, completion.error, completion.result]))) : ((matchValue_1 === 4) ? ((streamInvocation = message_1, new MsgPack_Msg$4(5, [streamInvocation.headers, streamInvocation.invocationId, streamInvocation.target, streamInvocation.arguments, streamInvocation.streamIds]))) : ((matchValue_1 === 5) ? ((cancelInvocation = message_1, new MsgPack_Msg$4(6, [cancelInvocation.headers, cancelInvocation.invocationId]))) : ((matchValue_1 === 6) ? (new MsgPack_Msg$4(7, [])) : ((matchValue_1 === 7) ? ((close = message_1, new MsgPack_Msg$4(8, [close.error, close.allowReconnect]))) : toFail(printf("Invalid message: %A"))(message_1)))))))), MsgPack_Msg$4_$reflection(unit_type, Action_$reflection(), unit_type, unit_type), outArr);
            if (compare(toUInt64(fromInt32(outArr.length)), 2147483648n) > 0) {
                throw new Error("Messages over 2GB are not supported.");
            }
            else {
                const msgArr = [];
                writeObject(toUInt64(fromInt32(outArr.length)), uint64_type, msgArr);
                addRangeInPlace(outArr, msgArr);
                return (new Uint8Array(msgArr)).buffer;
            }
        },
    })) : ((protocol_1 = Json_JsonProtocol_$ctor(), {
        name: "json",
        version: 1,
        transferFormat: 1,
        parseMessages(input_1, logger_3) {
            const input_2 = input_1;
            const logger_4 = logger_3;
            let collection_1;
            if (typeof input_2 === "string") {
                if (equals(input_2, "")) {
                    collection_1 = [];
                }
                else {
                    const str = input_2;
                    try {
                        collection_1 = choose((m) => {
                            let typeInfo_1, msg_4;
                            const parsedRaw = SimpleJson_parse(m);
                            let _arg_1;
                            const parsedRaw_1 = parsedRaw;
                            const msgType_1 = value_9(map((input_1_2) => {
                                const typeInfo = createTypeInfo(enum_type("Fable.SignalR.Messages.MessageType", int32_type, [["Invocation", 1], ["StreamItem", 2], ["Completion", 3], ["StreamInvocation", 4], ["CancelInvocation", 5], ["Ping", 6], ["Close", 7]]));
                                return Convert_fromJson(input_1_2, typeInfo) | 0;
                            }, SimpleJson_readPath(singleton("type"), parsedRaw))) | 0;
                            switch (msgType_1) {
                                case 1: {
                                    let _arg;
                                    try {
                                        _arg = (new FSharpResult$2(0, [(typeInfo_1 = createTypeInfo(HubRecords_InvocationMessage$1_$reflection(GameState_$reflection())), Convert_fromJson(parsedRaw_1, typeInfo_1))]));
                                    }
                                    catch (ex) {
                                        _arg = (new FSharpResult$2(1, [ex.message]));
                                    }
                                    if (_arg.tag === 1) {
                                        _arg_1 = Result_Map((arg_3) => {
                                            let msg_6;
                                            return (msg_6 = arg_3, ((msg_6.target === "") ? (() => {
                                                throw new Error("Invalid payload for Invocation message.");
                                            })() : void 0, ((msg_6.invocationId != null) ? ((value_9(msg_6.invocationId) === "") ? (() => {
                                                throw new Error("Invalid payload for Invocation message.");
                                            })() : void 0) : void 0, msg_6)));
                                        }, (() => {
                                            let typeInfo_2;
                                            try {
                                                return new FSharpResult$2(0, [(typeInfo_2 = createTypeInfo(HubRecords_InvocationMessage$1_$reflection(InvokeArg$1_$reflection(GameState_$reflection()))), Convert_fromJson(parsedRaw_1, typeInfo_2))]);
                                            }
                                            catch (ex_1) {
                                                return new FSharpResult$2(1, [ex_1.message]);
                                            }
                                        })());
                                    }
                                    else {
                                        const res = _arg.fields[0];
                                        _arg_1 = (new FSharpResult$2(0, [(msg_4 = res, ((msg_4.target === "") ? (() => {
                                            throw new Error("Invalid payload for Invocation message.");
                                        })() : void 0, ((msg_4.invocationId != null) ? ((value_9(msg_4.invocationId) === "") ? (() => {
                                            throw new Error("Invalid payload for Invocation message.");
                                        })() : void 0) : void 0, msg_4)))]));
                                    }
                                    break;
                                }
                                case 2: {
                                    _arg_1 = Result_Map((arg_5) => {
                                        let msg_7, matchValue_2, invocationId, invocationId_1;
                                        return (msg_7 = arg_5, (matchValue_2 = msg_7.invocationId, (matchValue_2 != null) ? (((invocationId = matchValue_2, invocationId !== "")) ? ((invocationId_1 = matchValue_2, msg_7)) : (() => {
                                            throw new Error("Invalid payload for StreamItem message.");
                                        })()) : (() => {
                                            throw new Error("Invalid payload for StreamItem message.");
                                        })()));
                                    }, (() => {
                                        let typeInfo_3;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_3 = createTypeInfo(HubRecords_StreamItemMessage$1_$reflection(unit_type)), Convert_fromJson(parsedRaw_1, typeInfo_3))]);
                                        }
                                        catch (ex_2) {
                                            return new FSharpResult$2(1, [ex_2.message]);
                                        }
                                    })());
                                    break;
                                }
                                case 3: {
                                    _arg_1 = Result_Map((arg_7) => {
                                        let msg_8, fail, matchValue_1_2, err;
                                        return (msg_8 = arg_7, (fail = (() => {
                                            throw new Error("Invalid payload for Completion message.");
                                        }), ((matchValue_1_2 = msg_8.error, (msg_8.result == null) ? ((matchValue_1_2 != null) ? ((err = matchValue_1_2, (err === "") ? fail() : void 0)) : ((msg_8.invocationId === "") ? fail() : void 0)) : ((matchValue_1_2 != null) ? fail() : ((msg_8.invocationId === "") ? fail() : void 0))), msg_8)));
                                    }, (() => {
                                        let typeInfo_4;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_4 = createTypeInfo(HubRecords_CompletionMessage$1_$reflection(GameState_$reflection())), Convert_fromJson(parsedRaw_1, typeInfo_4))]);
                                        }
                                        catch (ex_3) {
                                            return new FSharpResult$2(1, [ex_3.message]);
                                        }
                                    })());
                                    break;
                                }
                                case 4: {
                                    _arg_1 = Result_Map((arg_8) => arg_8, (() => {
                                        let typeInfo_5;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_5 = createTypeInfo(HubRecords_StreamInvocationMessage$1_$reflection(unit_type)), Convert_fromJson(parsedRaw_1, typeInfo_5))]);
                                        }
                                        catch (ex_4) {
                                            return new FSharpResult$2(1, [ex_4.message]);
                                        }
                                    })());
                                    break;
                                }
                                case 5: {
                                    _arg_1 = Result_Map((arg_9) => arg_9, (() => {
                                        let typeInfo_6;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_6 = createTypeInfo(HubRecords_CancelInvocationMessage_$reflection()), Convert_fromJson(parsedRaw_1, typeInfo_6))]);
                                        }
                                        catch (ex_5) {
                                            return new FSharpResult$2(1, [ex_5.message]);
                                        }
                                    })());
                                    break;
                                }
                                case 6: {
                                    _arg_1 = Result_Map((arg_10) => arg_10, (() => {
                                        let typeInfo_7;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_7 = createTypeInfo(HubRecords_PingMessage_$reflection()), Convert_fromJson(parsedRaw_1, typeInfo_7))]);
                                        }
                                        catch (ex_6) {
                                            return new FSharpResult$2(1, [ex_6.message]);
                                        }
                                    })());
                                    break;
                                }
                                case 7: {
                                    _arg_1 = Result_Map((arg_11) => arg_11, (() => {
                                        let typeInfo_8;
                                        try {
                                            return new FSharpResult$2(0, [(typeInfo_8 = createTypeInfo(HubRecords_CloseMessage_$reflection()), Convert_fromJson(parsedRaw_1, typeInfo_8))]);
                                        }
                                        catch (ex_7) {
                                            return new FSharpResult$2(1, [ex_7.message]);
                                        }
                                    })());
                                    break;
                                }
                                default:
                                    _arg_1 = toFail(printf("Invalid message: %A"))(parsedRaw_1);
                            }
                            if (_arg_1.tag === 1) {
                                const e_1 = _arg_1.fields[0];
                                const message_2 = toText(printf("Unknown message type: %s"))(e_1);
                                logger_4.log(4, message_2);
                                return void 0;
                            }
                            else {
                                const msg_9 = _arg_1.fields[0];
                                return msg_9;
                            }
                        }, Json_TextMessageFormat_parse(str));
                    }
                    catch (e_1_1) {
                        let message_1_1;
                        const arg_1_2 = e_1_1.message;
                        message_1_1 = toText(printf("An error occured during message deserialization: %s"))(arg_1_2);
                        logger_4.log(4, message_1_1);
                        collection_1 = [];
                    }
                }
            }
            else {
                logger_4.log(4, "Invalid input for JSON hub protocol. Expected a string, got an array buffer instead.");
                collection_1 = [];
            }
            return Array.from(collection_1);
        },
        writeMessage(msg_10) {
            let typeInfo_9;
            return Json_TextMessageFormat_write((typeInfo_9 = createTypeInfo(class_type("Fable.Core.U8`8", [class_type("Fable.SignalR.Messages.InvocationMessage`1", [Action_$reflection()]), class_type("Fable.SignalR.Messages.InvocationMessage`1", [InvokeArg$1_$reflection(Action_$reflection())]), class_type("Fable.SignalR.Messages.StreamItemMessage`1", [unit_type]), class_type("Fable.SignalR.Messages.CompletionMessage`1", [Action_$reflection()]), class_type("Fable.SignalR.Messages.StreamInvocationMessage`1", [unit_type]), class_type("Fable.SignalR.Messages.CancelInvocationMessage"), class_type("Fable.SignalR.Messages.PingMessage"), class_type("Fable.SignalR.Messages.CloseMessage")])), Convert_serialize(msg_10, typeInfo_9)));
        },
    }))), _["hub@10"].withHubProtocol(protocol_2).build()), _.handlers);
})();

HubConnection$5__startNow(signalRHub);

HubConnection$5__sendNow_2B595(signalRHub, new Action(0, []));

export function mouseclick(event) {
    const rect = canvas.getBoundingClientRect();
    const pos = new Position((event.x - rect.left) * 1, (event.y - rect.top) * 1);
    const isHover = (hex) => HexagonModule_isInside(pos, hex);
    const foundHexagon = tryFind(isHover, allHexagons());
    selectField(foundHexagon);
    PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (HubConnection$5__sendAsPromise_2B595(signalRHub, new Action(2, [playerId, "Test"])).then(() => (HubConnection$5__sendAsPromise_2B595(signalRHub, new Action(1, [playerId, new BotType(2, [])])).then(() => (HubConnection$5__sendAsPromise_2B595(signalRHub, new Action(1, [playerId, new BotType(2, [])])).then(() => (HubConnection$5__sendAsPromise_2B595(signalRHub, new Action(7, [playerId])).then(() => (Promise.resolve(void 0))))))))))));
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
        let selectedFieldId;
        if (selectedField() == null) {
            selectedFieldId = -1;
        }
        else {
            const field = selectedField();
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
                        for (let idx_1 = 0; idx_1 <= (hexagons.length - 1); idx_1++) {
                            const hexagon = hexagons[idx_1];
                            Animation_apply(uncurry2(method), ctx, group, hexagon);
                        }
                    }
                };
                apply((animation) => ((tupledArg) => {
                    animation.AnimateHexagon(tupledArg[0], tupledArg[1], tupledArg[2]);
                }));
                apply((animation_1) => ((tupledArg_1) => {
                    animation_1.AnimateOuterEdge(tupledArg_1[0], tupledArg_1[1], tupledArg_1[2]);
                }));
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    }
    return window.requestAnimationFrame((arg_6) => {
        draw(arg_6);
    });
}

draw(0);

