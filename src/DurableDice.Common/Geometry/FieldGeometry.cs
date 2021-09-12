using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry;

public class FieldGeometry
{
    private readonly IEnumerable<Field> _fields;

    public FieldGeometry(IEnumerable<Field> fields)
    {
        _fields = fields;
    }

    public bool AreNeighboringFields(string fieldId1, string fieldId2)
    {
        var field1 = _fields.FirstOrDefault(x => x.Id == fieldId1);
        var field2 = _fields.FirstOrDefault(x => x.Id == fieldId2);

        if (field1 == null ||
            field2 == null ||
            field1.Coordinates.Any(c1 => field2.Coordinates.Any(c2 => c1 == c2)))
        {
            return false;
        }

        // TODO: determine border coordinates to reduce number of comparisons
        foreach (var field1NeighboringCoordinate in field1.Coordinates.SelectMany(GetNeighboringCoordinates))
        {
            if (field2.Coordinates.Any(field2Coordinate => field1NeighboringCoordinate == field2Coordinate))
            {
                return true;
            }
        }

        return false;
    }

    public int GetLargestContinuousFieldBlock(string ownerId)
    {
        var ownedFields = _fields.Where(x => x.OwnerId == ownerId);

        var neighborMap = ownedFields.Aggregate(
            new Dictionary<string, IReadOnlyList<string>>(),
            (aggregate, field) =>
            {
                aggregate[field.Id] = ownedFields
                    .Where(otherField => AreNeighboringFields(field.Id, otherField.Id))
                    .Select(otherField => otherField.Id)
                    .Append(field.Id)
                    .ToList();

                return aggregate;
            });

        var gotBigger = false;
        do
        {
            gotBigger = false;

            neighborMap = neighborMap.Aggregate(
                new Dictionary<string, IReadOnlyList<string>>(),
                (aggregate, data) =>
                {
                    aggregate[data.Key] = data.Value
                        .SelectMany(field => neighborMap[field])
                        .Concat(data.Value)
                        .Distinct()
                        .ToList();

                    gotBigger = gotBigger || data.Value.Count < aggregate[data.Key].Count;

                    return aggregate;
                });
        }
        while (gotBigger);

        return neighborMap.Values
            .OrderByDescending(x => x.Count)
            .FirstOrDefault()?.Count ?? 0;
    }

    public static IEnumerable<Coordinate> GetNeighboringCoordinates(Coordinate center)
    {
        if (center.X % 2 == 1)
        {
            yield return new Coordinate(center.X, center.Y - 1);
            yield return new Coordinate(center.X - 1, center.Y);
            yield return new Coordinate(center.X - 1, center.Y + 1);
            yield return new Coordinate(center.X, center.Y + 1);
            yield return new Coordinate(center.X + 1, center.Y + 1);
            yield return new Coordinate(center.X + 1, center.Y);
        }
        else
        {
            yield return new Coordinate(center.X, center.Y - 1);
            yield return new Coordinate(center.X - 1, center.Y - 1);
            yield return new Coordinate(center.X - 1, center.Y);
            yield return new Coordinate(center.X, center.Y + 1);
            yield return new Coordinate(center.X + 1, center.Y);
            yield return new Coordinate(center.X + 1, center.Y - 1);
        }
    }

    public static List<Coordinate> GetCircleAroundCoordinate(Coordinate center, int radius)
    {
        var coordinates = new List<Coordinate>
        {
            center
        };

        if (radius > 0)
        {
            var neighbors = GetNeighboringCoordinates(center);
            coordinates.AddRange(neighbors.SelectMany(neighbor => GetCircleAroundCoordinate(neighbor, radius - 1)));
        }

        return coordinates.Distinct().ToList();
    }

    public static List<Coordinate> GetShapeAroundCoordinate(Coordinate center, int size, IReadOnlyList<Coordinate> allowedCoordinates)
    {
        var coordinates = new List<Coordinate>
        {
            center
        };

        if (size > 0)
        {
            var neighbors = GetNeighboringCoordinates(center)
                .Where(allowedCoordinates.Contains)
                .OrderBy(x => Guid.NewGuid())
                .Take(Random.Shared.Next(1, 3));

            coordinates.AddRange(neighbors.SelectMany(neighbor => GetShapeAroundCoordinate(neighbor, size - 1, allowedCoordinates)));
        }

        return coordinates.Distinct().ToList();
    }
}
