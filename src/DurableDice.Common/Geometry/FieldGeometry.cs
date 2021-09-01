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
                    var otherFields = ownedFields.Where(x => x.Id != field.Id);

                    aggregate[field.Id] = otherFields
                        .Where(otherField => AreNeighboringFields(field.Id, otherField.Id))
                        .Select(otherField => otherField.Id)
                        .ToList();

                    return aggregate;
                });

        return 1 + neighborMap
            .Keys
            .Select(field => CountNumberOfNeighbors(field, Enumerable.Empty<string>(), neighborMap))
            .OrderByDescending(x => x)
            .FirstOrDefault();
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

    private int CountNumberOfNeighbors(
        string fieldId,
        IEnumerable<string> fieldsIncludedInCount,
        IReadOnlyDictionary<string, IReadOnlyList<string>> map)
    {
        var neighbors = map[fieldId].Where(field => !fieldsIncludedInCount.Contains(field)).ToArray();
        return neighbors.Length + neighbors.Sum(neighbor => CountNumberOfNeighbors(neighbor, fieldsIncludedInCount.Append(fieldId), map));
    }
}
