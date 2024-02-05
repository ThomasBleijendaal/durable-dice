import { Record } from "./fable_modules/fable-library.4.11.0/Types.js";
import { record_type, array_type, int32_type } from "./fable_modules/fable-library.4.11.0/Reflection.js";
import { EdgeTypeModule_calculateEdge, PositionModule_calculateEdges, CoordinateModule_toPosition, CoordinateModule_neighbors, Coordinate_$reflection } from "./Models.fs.js";
import { append, collect, contains, map } from "./fable_modules/fable-library.4.11.0/Array.js";
import { safeHash, equals } from "./fable_modules/fable-library.4.11.0/Util.js";
import { FieldHexagons, HexagonModule_coordinate, Hexagon } from "./Hexagon.fs.js";
import { Array_distinct } from "./fable_modules/fable-library.4.11.0/Seq2.js";
import { color } from "./Theme.fs.js";

export class Field extends Record {
    constructor(Id, PlayerId, DiceCount, DiceAdded, Coordinates, Center) {
        super();
        this.Id = (Id | 0);
        this.PlayerId = (PlayerId | 0);
        this.DiceCount = (DiceCount | 0);
        this.DiceAdded = (DiceAdded | 0);
        this.Coordinates = Coordinates;
        this.Center = Center;
    }
}

export function Field_$reflection() {
    return record_type("Field.Field", [], Field, () => [["Id", int32_type], ["PlayerId", int32_type], ["DiceCount", int32_type], ["DiceAdded", int32_type], ["Coordinates", array_type(Coordinate_$reflection())], ["Center", Coordinate_$reflection()]]);
}

export function FieldModule_groupHexagons(field) {
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
                        return contains(c_1, cs, {
                            Equals: equals,
                            GetHashCode: safeHash,
                        }) === false;
                    });
                    const unprocessedNeighborsOfCoordinate = map((tuple) => tuple[1], neighborsOfCoordinate.filter((tupledArg_1) => {
                        const c_2 = tupledArg_1[1];
                        if (contains(c_2, processedCoordinates, {
                            Equals: equals,
                            GetHashCode: safeHash,
                        }) === false) {
                            return contains(c_2, cs, {
                                Equals: equals,
                                GetHashCode: safeHash,
                            }) === true;
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
                const nextNeighborsToProcess = Array_distinct(collect((x_3) => x_3, map((tuple_3) => tuple_3[1], coordinateNeighbors)), {
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
            const outGroup = cs.filter((c_4) => (contains(c_4, map(HexagonModule_coordinate, coordinatesWithEdges), {
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
    return new FieldHexagons(field.Id, field.Center, CoordinateModule_toPosition(field.Center), color(field.PlayerId), hexagons);
}

