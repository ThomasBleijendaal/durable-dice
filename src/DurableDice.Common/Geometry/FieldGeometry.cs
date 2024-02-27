using System.Security.Cryptography;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry;

public static class FieldGeometry
{
    public static bool AreNeighboringFields(IEnumerable<Field> fields, string fieldId1, string fieldId2)
        => AreNeighboringFields(fields.FirstOrDefault(x => x.Id == fieldId1), fields.FirstOrDefault(x => x.Id == fieldId2));

    public static bool AreNeighboringFields(Field? field1, Field? field2)
    {
        if (field1 == null ||
            field2 == null ||
            field1.Coordinates.Any(c1 => field2.Coordinates.Any(c2 => c1 == c2)))
        {
            return false;
        }

        return field1.Coordinates
            .SelectMany(GetNeighboringCoordinates)
            .Any(field1NeighboringCoordinate => field2.Coordinates.Any(field2Coordinate => field1NeighboringCoordinate == field2Coordinate));
    }

    public static int GetLargestContinuousFieldBlock(IEnumerable<Field> fields, string ownerId)
        => GetLargestContinuousFieldBlock(fields.Where(x => x.OwnerId == ownerId));

    public static int GetLargestContinuousFieldBlock(IEnumerable<Field> fields)
    {
        var neighborMap = fields.Aggregate(
            new Dictionary<string, IReadOnlyList<string>>(),
            (aggregate, field) =>
            {
                aggregate[field.Id] = fields
                    .Where(otherField => AreNeighboringFields(field, otherField))
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

    public static IEnumerable<Coordinate> GetNeighboringCoordinates(Coordinate c)
    {
        // TODO: this math does not work with V2

        // V2

        return (c.Y % 2) switch
        {
            1 => new Coordinate[]
            {
                new(c.X - 1, c.Y - 1),
                new(c.X - 1, c.Y),
                new(c.X - 1, c.Y + 1),
                new(c.X, c.Y + 1),
                new(c.X + 1, c.Y),
                new(c.X, c.Y - 1)
            },
            _ => new Coordinate[]
            {
                new(c.X, c.Y - 1),
                new(c.X - 1, c.Y),
                new(c.X, c.Y + 1),
                new(c.X + 1, c.Y + 1),
                new(c.X + 1, c.Y),
                new(c.X + 1, c.Y - 1)
            },
        };

        // V1
        //if (center.X % 2 == 1)
        //{
        //    yield return new Coordinate(center.X, center.Y - 1);
        //    yield return new Coordinate(center.X - 1, center.Y);
        //    yield return new Coordinate(center.X - 1, center.Y + 1);
        //    yield return new Coordinate(center.X, center.Y + 1);
        //    yield return new Coordinate(center.X + 1, center.Y + 1);
        //    yield return new Coordinate(center.X + 1, center.Y);
        //}
        //else
        //{
        //    yield return new Coordinate(center.X, center.Y - 1);
        //    yield return new Coordinate(center.X - 1, center.Y - 1);
        //    yield return new Coordinate(center.X - 1, center.Y);
        //    yield return new Coordinate(center.X, center.Y + 1);
        //    yield return new Coordinate(center.X + 1, center.Y);
        //    yield return new Coordinate(center.X + 1, center.Y - 1);
        //}
    }

    public static List<Coordinate> GetCircleAroundCoordinate(Coordinate origin, int radius)
    {
        var coordinates = new List<Coordinate>
        {
            origin
        };

        if (radius > 0)
        {
            var neighbors = GetNeighboringCoordinates(origin);
            coordinates.AddRange(neighbors.SelectMany(neighbor => GetCircleAroundCoordinate(neighbor, radius - 1)));
        }

        return coordinates.Distinct().ToList();
    }

    public static List<Coordinate> GetShapeAroundCoordinate(Coordinate origin, int size, IReadOnlyList<Coordinate> allowedCoordinates)
    {
        var coordinates = new List<Coordinate>
        {
            origin
        };

        if (size > 0)
        {
            var neighbors = GetNeighboringCoordinates(origin)
                .Where(allowedCoordinates.Contains)
                .OrderBy(x => Guid.NewGuid())
                .Take(RandomNumberGenerator.GetInt32(3) + 1);

            coordinates.AddRange(neighbors.SelectMany(neighbor => GetShapeAroundCoordinate(neighbor, size - 1, allowedCoordinates)));
        }

        return coordinates.Distinct().ToList();
    }

    public static Coordinate GetMostCentralCoordinate(IReadOnlyList<Coordinate> coordinates)
    {
        var distances = coordinates
            .Select(c => (coordinate: c, distance: MaxDistance(coordinates, c)))
            .OrderByDescending(x => x.distance)
            .ToArray();

        // if all distances are equal (i.e. weird shape with no big internal volume)
        if (distances.GroupBy(x => x.distance).Count() == 1)
        {
            distances = coordinates
                .Select(c => (coordinate: c, distance: TotalDistance(coordinates, c)))
                .OrderBy(x => x.distance)
                .ToArray();
        }

        return distances[0].coordinate;
    }

    private static double TotalDistance(IReadOnlyList<Coordinate> coordinates, Coordinate c)
    {
        return coordinates.Sum(o => Math.Pow(Math.Abs(c.X - o.X), 2) + Math.Pow(Math.Abs(c.Y - o.Y), 2));
    }

    private static double MaxDistance(IReadOnlyList<Coordinate> coordinates, Coordinate c)
    {
        return Enumerable.Range(0, 6)
            .Select(dir => GetDistance(c, coordinates, dir))
            .Min();
    }

    private static int GetDistance(Coordinate coordinate, IReadOnlyList<Coordinate> coordinates, int direction)
    {
        var neighborInDirection = GetNeighboringCoordinates(coordinate).ElementAt(direction);
        if (coordinates.Contains(neighborInDirection))
        {
            return 1 + GetDistance(neighborInDirection, coordinates, direction);
        }
        else
        {
            return 0;
        }
    }
}
