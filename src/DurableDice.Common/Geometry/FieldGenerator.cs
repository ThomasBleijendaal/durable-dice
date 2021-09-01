using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry;

public static class FieldGenerator
{
    private const int HorizontalCoordinates = 60;
    private const int VerticalCoordinates = 40;

    private static readonly Random Random = new Random();

    public static List<Field> GenerateFields(IReadOnlyList<Player> players)
    {
        var fields = new List<Field>();

        if (players.Count <= 1)
        {
            return fields;
        }

        var fieldsPerPlayer = 32 / players.Count;
        var totalFields = fieldsPerPlayer * players.Count;

        var coordinates = Enumerable.Range(0, VerticalCoordinates)
            .SelectMany(y => Enumerable.Range(0, HorizontalCoordinates).Select(x => new Coordinate(x, y)))
            .ToList();

        //var holes = Enumerable.Range(0, _random.Next(10, 21)).Select(i => new Coordinate(_random.Next(0, HorizontalCoordinates), _random.Next(0, VerticalCoordinates)));

        //var allCoordinates = coordinates.Except(holes);

        var coordinateIndex = 0;

        foreach (var player in players)
        {
            fields.AddRange(
                Enumerable.Range(0, fieldsPerPlayer).Select(_ => new Field
                {
                    Coordinates = coordinates.Skip(coordinateIndex += 3).Take(3).ToList(),
                    DiceCount = 1,
                    Id = $"{coordinateIndex}",
                    OwnerId = player.Id
                }));
        }

        // create 10 center fields


        // take 8 corner fields

        // add the rest


        return fields;
    }


}
