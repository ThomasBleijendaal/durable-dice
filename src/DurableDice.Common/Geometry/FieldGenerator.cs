using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry;

public static class FieldGenerator
{
    private const int HorizontalCoordinates = 30;
    private const int VerticalCoordinates = 20;

    private static readonly Random Random = new Random();

    public static List<Field> GenerateFields(IReadOnlyList<Player> players)
    {
        if (players.Count <= 1)
        {
            return new List<Field>();
        }

        var fieldsPerPlayer = 32 / players.Count;
        var totalFields = fieldsPerPlayer * players.Count;

        var coordinates = Enumerable.Range(0, VerticalCoordinates)
            .SelectMany(y => Enumerable.Range(0, HorizontalCoordinates).Select(x => new Coordinate(x, y)))
            .OrderBy(x => Guid.NewGuid())
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
            int radius;
            if (first)
            {
                center = coordinates.First();
                radius = Random.Next(1, 4);
                first = false;
            }
            else
            {
                radius = Random.Next(1, 3);
                center = Neighbors(allCoordinates, radius)
                    .Intersect(coordinates)
                    .OrderBy(x => Guid.NewGuid())
                    .First();
            }

            var block = FieldGeometry.GetCircleAroundCoordinate(center, radius);
            var allowedBlocks = block.Intersect(coordinates).ToList();

            field.Coordinates = allowedBlocks;
            field.Center = center;

            allowedBlocks.ForEach(claimedBlock => coordinates.Remove(claimedBlock));

            static IEnumerable<Coordinate> Neighbors(IEnumerable<Coordinate> coordinates, int distance) 
                => distance == 0
                    ? coordinates
                    : Neighbors(coordinates.SelectMany(FieldGeometry.GetNeighboringCoordinates), distance - 1);
        }

        return playerFields;
    }
}
