using System.Security.Cryptography;
using DurableDice.Common.Models.State;
using System.Linq;

namespace DurableDice.Common.Geometry;

public static class FieldGenerator
{
    private const int HorizontalCoordinates = 30;
    private const int VerticalCoordinates = 20;

    public static List<Field> GenerateFields(IReadOnlyList<Player> players)
    {
        if (players.Count <= 1)
        {
            return new List<Field>();
        }

        var fieldsPerPlayer = 32 / players.Count;

        var coordinates = Enumerable.Range(0, VerticalCoordinates)
            .SelectMany(y => Enumerable.Range(0, HorizontalCoordinates).Select(x => new Coordinate(x, y)))
            .OrderBy(x => Guid.NewGuid())
            .Skip((VerticalCoordinates / 3) * (HorizontalCoordinates / 3))
            .ToList();

        var playerFields = players
            .SelectMany(player =>
                Enumerable.Range(0, fieldsPerPlayer)
                    .Select(field => new Field
                    {
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
            Coordinate center;
            if (first)
            {
                center = coordinates.First(x => 
                    x.X > (HorizontalCoordinates / 3) && x.X < (2 * HorizontalCoordinates / 3) &&
                    x.Y > (VerticalCoordinates / 3) && x.Y < (2 * VerticalCoordinates / 3));
                first = false;
            }
            else
            {
                center = Neighbors(allCoordinates, 1)
                    .Intersect(coordinates)
                    .OrderBy(x => Guid.NewGuid())
                    .First();
            }

            var size = RandomNumberGenerator.GetInt32(5) + 1;
            var block = FieldGeometry.GetShapeAroundCoordinate(center, size, coordinates);
            var allowedBlocks = block.Intersect(coordinates).ToList();

            field.Coordinates = allowedBlocks;
            field.Center = center;

            allowedBlocks.ForEach(claimedBlock => coordinates.Remove(claimedBlock));

            static IEnumerable<Coordinate> Neighbors(IEnumerable<Coordinate> coordinates, int distance)
                => distance == 0
                    ? coordinates
                    : Neighbors(coordinates.SelectMany(FieldGeometry.GetNeighboringCoordinates), distance - 1);
        }

        foreach (var (field, index) in playerFields.Select((field, index) => (field, index)))
        {
            field.Index = index;

            field.Neighbors.AddRange(playerFields
                .Select((field, index) => (field, index))
                .Where(neightborField => FieldGeometry.AreNeighboringFields(field, neightborField.field))
                .Select(neightborField => neightborField.index));
        }

        return playerFields;
    }
}
