using DurableDice.Common.Geometry;

namespace DurableDice.Common.Models.State;

internal class BotState
{
    private readonly FieldGeometry _geometry;

    public BotState(string playerId, IReadOnlyList<Field> fields)
    {
        PlayerId = playerId;
        Fields = fields;

        _geometry = new(fields);
    }

    public string PlayerId { get; }
    public IReadOnlyList<Field> Fields { get; }

    public IEnumerable<Field> OwnFields => Fields.Where(x => x.OwnerId == PlayerId);
    public IEnumerable<Field> AttackingOwnFields => Fields.Where(x => x.OwnerId == PlayerId && x.DiceCount > 1);

    public IEnumerable<Field> AttackableNeighboringEnemyFields => Fields
        .Except(OwnFields)
        .Where(x => AttackingOwnFields.Any(o => _geometry.AreNeighboringFields(o.Id, x.Id)));

    public IEnumerable<Field> StrongestFieldNear(Field enemyField) => OwnFields
        .Where(x => _geometry.AreNeighboringFields(x.Id, enemyField.Id))
        .OrderByDescending(x => x.DiceCount);

    public IEnumerable<Field> VulnerableFields
    {
        get
        {
            var currentFieldSize = _geometry.GetLargestContinuousFieldBlock(PlayerId);

            return OwnFields.OrderByDescending(field =>
                new FieldGeometry(OwnFields.Except(new[] { field }))
                    .GetLargestContinuousFieldBlock(PlayerId) / currentFieldSize);
        }
    }

    public IEnumerable<Field> MostAdvantagousAttackableNeighboringEnemyFields
    {
        get
        {
            var currentFieldSize = _geometry.GetLargestContinuousFieldBlock(PlayerId);

            return AttackableNeighboringEnemyFields
                .OrderByDescending(field =>
                    new FieldGeometry(OwnFields.Append(field.Copy(PlayerId)))
                        .GetLargestContinuousFieldBlock(PlayerId) / currentFieldSize);
        }
    }
}
