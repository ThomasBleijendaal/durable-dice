using System.Security.Cryptography;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry;

public static class FieldGenerator
{
    private const int HorizontalCoordinates = 60;
    private const int VerticalCoordinates = 60;

    public static List<Field> GenerateFields(IReadOnlyList<Player> players)
    {
        if (players.Count <= 1)
        {
            return new List<Field>();
        }

        var fieldsPerPlayer = 48 / players.Count;

        var coordinates = Enumerable.Range(0, VerticalCoordinates)
            .SelectMany(y => Enumerable.Range(0, HorizontalCoordinates).Select(x => new Coordinate(x, y)))
            .OrderBy(x => Guid.NewGuid())
            // TODO: restore
            //.Skip(VerticalCoordinates / 3 * (HorizontalCoordinates / 3))
            .ToList();

        var index = 0;

        var playerFields = players
            .SelectMany(player =>
                Enumerable.Range(0, fieldsPerPlayer)
                    .Select(field => new Field
                    {
                        Index = index++,
                        DiceCount = 1,
                        Id = $"{player.Id}-{field}",
                        OwnerId = player.Id
                    }))
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        var allCoordinates = playerFields.SelectMany(x => x.Coordinates);

        var first = true;

        foreach (var field in playerFields)
        {
            Coordinate origin;
            if (first)
            {
                origin = coordinates.First(x =>
                    x.X > (2 * HorizontalCoordinates / 4) && x.X < (3 * HorizontalCoordinates / 4) &&
                    x.Y > (2 * VerticalCoordinates / 4) && x.Y < (3 * VerticalCoordinates / 4));
                first = false;
            }
            else
            {
                origin = Neighbors(allCoordinates, 1)
                    .Intersect(coordinates)
                    .OrderBy(x => Guid.NewGuid())
                    .First();
            }

            var size = RandomNumberGenerator.GetInt32(6) + 4;
            var block = FieldGeometry.GetShapeAroundCoordinate(origin, size, coordinates);
            var allowedBlocks = block.Intersect(coordinates).ToList();

            field.Coordinates = allowedBlocks;

            field.Center = FieldGeometry.GetMostCentralCoordinate(allowedBlocks);

            allowedBlocks.ForEach(claimedBlock => coordinates.Remove(claimedBlock));

            static IEnumerable<Coordinate> Neighbors(IEnumerable<Coordinate> coordinates, int distance)
                => distance == 0
                    ? coordinates
                    : Neighbors(coordinates.SelectMany(FieldGeometry.GetNeighboringCoordinates), distance - 1);
        }

        foreach (var field in playerFields)
        {
            field.Neighbors.AddRange(playerFields
                .Where(neightborField => FieldGeometry.AreNeighboringFields(field, neightborField))
                .Select(neightborField => neightborField.Index));
        }

        return playerFields;
    }
}
