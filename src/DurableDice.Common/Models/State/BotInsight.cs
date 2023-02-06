using DurableDice.Common.Geometry;

namespace DurableDice.Common.Models.State;

internal class BotInsight
{
    public BotInsight(GameState gameState, IReadOnlyList<Field> fields)
    {
        GameState = gameState;
        Fields = fields;
    }

    public string ActivePlayerId => GameState.ActivePlayerId ?? throw new ArgumentException("Invalid game state");

    public GameState GameState { get; }
    public IReadOnlyList<Field> Fields { get; }

    public IEnumerable<Field> OwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId);
    public IEnumerable<Field> AttackingOwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId && x.DiceCount > 1);

    public IEnumerable<Field> AttackableNeighboringEnemyFields => Fields
        .Except(OwnFields)
        .Where(x => AttackingOwnFields.Any(o => FieldGeometry.AreNeighboringFields(o, x)));

    public IEnumerable<Field> NeighboringEnemyFields => Fields
        .Except(OwnFields)
        .Where(x => OwnFields.Any(o => FieldGeometry.AreNeighboringFields(o, x)));

    public IEnumerable<Field> SafeFields => OwnFields
        .Where(field => NeighboringEnemyFields.All(n => !FieldGeometry.AreNeighboringFields(n, field) || n.DiceCount == 1));

    public IEnumerable<Field> StrongestFieldNear(Field field) => OwnFields
        .Where(x => FieldGeometry.AreNeighboringFields(x, field))
        .OrderByDescending(x => x.DiceCount);

    public IEnumerable<Field> VulnerableFields
    {
        get
        {
            var currentFieldSize = FieldGeometry.GetLargestContinuousFieldBlock(Fields, ActivePlayerId);

            var order = OwnFields
                .Except(SafeFields)
                .OrderBy(field =>
                    FieldGeometry.GetLargestContinuousFieldBlock(OwnFields.Except(new[] { field })) / currentFieldSize)
                .ToList();

            return order;
        }
    }

    public IEnumerable<Field> MostAdvantagousAttackableNeighboringEnemyFields
    {
        get
        {
            var currentFieldSize = FieldGeometry.GetLargestContinuousFieldBlock(Fields, ActivePlayerId);

            return AttackableNeighboringEnemyFields
                .OrderBy(field =>
                    FieldGeometry.GetLargestContinuousFieldBlock(OwnFields.Append(field.Copy(ActivePlayerId))) / currentFieldSize);
        }
    }
}
