import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { record_type, array_type, string_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { EdgeTypeModule_calculateEdge, PositionModule_calculateEdges, CoordinateModule_toPosition, CoordinateModule_isSame, CoordinateModule_neighbors, Coordinate_$reflection } from "./Models.fs.js";
import { contains, append, collect, tryFind, map } from "./fable_modules/fable-library.4.11.0/Array.js";
import { safeHash, equals } from "./fable_modules/fable-library.4.11.0/Util.js";
import { FieldHexagons, HexagonModule_coordinate, Hexagon } from "./Hexagon.fs.js";
import { Array_distinct } from "./fable_modules/fable-library.4.11.0/Seq2.js";

export class Field extends Record {
    constructor(Index, Id, OwnerId, DiceCount, DiceAdded, Coordinates, Center, Neighbors) {
        super();
        this.Index = (Index | 0);
        this.Id = Id;
        this.OwnerId = OwnerId;
        this.DiceCount = (DiceCount | 0);
        this.DiceAdded = (DiceAdded | 0);
        this.Coordinates = Coordinates;
        this.Center = Center;
        this.Neighbors = Neighbors;
    }
}

export function Field_$reflection() {
    return record_type("Field.Field", [], Field, () => [["Index", int32_type], ["Id", string_type], ["OwnerId", string_type], ["DiceCount", int32_type], ["DiceAdded", int32_type], ["Coordinates", array_type(Coordinate_$reflection())], ["Center", Coordinate_$reflection()], ["Neighbors", array_type(int32_type)]]);
}

export function FieldModule_groupHexagons(players, field) {
    const loopCoordinates = (cs) => {
        if (cs.length === 0) {
            return [];
        }
        else {
            const loop = (processedCoordinates, nextCoordinates) => {
                const coordinateNeighbors = map((coordinate) => {
                    const neighborsOfCoordinate = CoordinateModule_neighbors(coordinate);
                    const externalEdges = neighborsOfCoordinate.filter((tupledArg) => {
                        const c_1 = tupledArg[1];
                        return equals(tryFind((c_2) => CoordinateModule_isSame(c_1, c_2), cs), void 0);
                    });
                    const unprocessedNeighborsOfCoordinate = map((tuple) => tuple[1], neighborsOfCoordinate.filter((tupledArg_1) => {
                        const c_3 = tupledArg_1[1];
                        if (equals(tryFind((c_4) => CoordinateModule_isSame(c_3, c_4), processedCoordinates), void 0)) {
                            return !equals(tryFind((c_5) => CoordinateModule_isSame(c_3, c_5), cs), void 0);
                        }
                        else {
                            return false;
                        }
                    }));
                    const position = CoordinateModule_toPosition(coordinate);
                    let hex;
                    const AllEdges = PositionModule_calculateEdges(position);
                    hex = (new Hexagon(field.Id, coordinate, position, map((tuple_1) => tuple_1[0], externalEdges), AllEdges, map((tupledArg_2) => {
                        const edgeType = tupledArg_2[0];
                        return EdgeTypeModule_calculateEdge(edgeType)(position);
                    }, externalEdges)));
                    return [hex, unprocessedNeighborsOfCoordinate];
                }, nextCoordinates);
                const coordinateEdges = map((tuple_2) => tuple_2[0], coordinateNeighbors);
                const nextNeighborsToProcess = Array_distinct(collect((x) => x, map((tuple_3) => tuple_3[1], coordinateNeighbors)), {
                    Equals: equals,
                    GetHashCode: safeHash,
                });
                const processedCoordinates_1 = append(nextNeighborsToProcess, append(map(HexagonModule_coordinate, coordinateEdges), processedCoordinates));
                if (nextNeighborsToProcess.length === 0) {
                    return coordinateEdges;
                }
                else {
                    return append(loop(processedCoordinates_1, nextNeighborsToProcess), coordinateEdges);
                }
            };
            const firstCoordinate = [cs[0]];
            const coordinatesWithEdges = loop(firstCoordinate, firstCoordinate);
            const outGroup = cs.filter((c_7) => (contains(c_7, map(HexagonModule_coordinate, coordinatesWithEdges), {
                Equals: equals,
                GetHashCode: safeHash,
            }) === false));
            if (outGroup.length === 0) {
                return [coordinatesWithEdges];
            }
            else {
                const array_23 = [coordinatesWithEdges];
                return append(loopCoordinates(outGroup), array_23);
            }
        }
    };
    const hexagons = loopCoordinates(field.Coordinates);
    return new FieldHexagons(field.Id, field.Center, CoordinateModule_toPosition(field.Center), hexagons);
}

