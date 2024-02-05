module Field

open Theme
open Models
open Hexagon

type Field =
    { Id: int
      PlayerId: int

      DiceCount: int
      DiceAdded: int

      Coordinates: Coordinate array
      Center: Coordinate }

type Fields = Field array

module Field =
    let groupHexagons (field: Field) : FieldHexagons =
        
        let rec loopCoordinates (cs: Coordinates) = 
            if cs.Length = 0 then
                [||]
            else
                let rec loop (processedCoordinates: Coordinates) (nextCoordinates: Coordinates) =
                    let coordinateNeighbors =
                        nextCoordinates
                        |> Array.map (fun coordinate ->
                            let neighborsOfCoordinate = coordinate |> Coordinate.neighbors

                            let externalEdges =
                                neighborsOfCoordinate
                                |> Array.filter (fun (_, c) -> cs |> Array.contains c = false)

                            let unprocessedNeighborsOfCoordinate =
                                neighborsOfCoordinate
                                |> Array.filter (fun (_, c) ->
                                    processedCoordinates |> Array.contains c = false
                                    && cs |> Array.contains c = true)
                                |> Array.map snd

                            let position = coordinate |> Coordinate.toPosition

                            let hex: Hexagon = { 
                                FieldId = field.Id
                                Coordinate = coordinate
                                Position = position
                                AllEdges = position |> Position.calculateEdges
                                EdgeTypes = externalEdges |> Array.map fst
                                OuterEdges = externalEdges |> Array.map (fun (edgeType, _) -> EdgeType.calculateEdge edgeType position) }

                            hex, unprocessedNeighborsOfCoordinate)

                    let coordinateEdges = coordinateNeighbors |> Array.map fst

                    let nextNeighborsToProcess =
                        coordinateNeighbors |> Array.map snd |> Array.collect id |> Array.distinct

                    let processedCoordinates =
                        processedCoordinates
                        |> Array.append (coordinateEdges |> Array.map Hexagon.coordinate)
                        |> Array.append nextNeighborsToProcess

                    if nextNeighborsToProcess.Length = 0 then
                        coordinateEdges
                    else
                        coordinateEdges
                        |> Array.append (loop processedCoordinates nextNeighborsToProcess)

                let firstCoordinate = [| cs[0] |]

                let coordinatesWithEdges = loop firstCoordinate firstCoordinate

                let outGroup =
                    cs
                    |> Array.filter (fun c -> coordinatesWithEdges |> Array.map Hexagon.coordinate |> Array.contains c = false)

                if outGroup.Length = 0 then
                    [| coordinatesWithEdges |]
                else
                    [| coordinatesWithEdges |] |> Array.append (loopCoordinates outGroup)

        let hexagons = loopCoordinates field.Coordinates

        { 
            FieldId = field.Id
            Center = field.Center
            CenterPosition = field.Center |> Coordinate.toPosition

            Color = Theme.color(Some field.PlayerId)

            Hexagons = hexagons
        }