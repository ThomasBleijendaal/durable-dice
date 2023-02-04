using DurableDice.Common.Geometry;

namespace DurableDice.Common.Models.State;

internal class BotInsight
{
    private readonly FieldGeometry _geometry;

    public BotInsight(GameState gameState, IReadOnlyList<Field> fields)
    {
        GameState = gameState;
        Fields = fields;

        _geometry = new(fields);
    }

    public string ActivePlayerId => GameState.ActivePlayerId ?? throw new ArgumentException("Invalid game state");

    public GameState GameState { get; }
    public IReadOnlyList<Field> Fields { get; }

    public IEnumerable<Field> OwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId);
    public IEnumerable<Field> AttackingOwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId && x.DiceCount > 1);

    public IEnumerable<Field> AttackableNeighboringEnemyFields => Fields
        .Except(OwnFields)
        .Where(x => AttackingOwnFields.Any(o => _geometry.AreNeighboringFields(o.Id, x.Id)));

    public IEnumerable<Field> SafeFields => OwnFields
        .Where(field => AttackableNeighboringEnemyFields.All(n => !_geometry.AreNeighboringFields(n.Id, field.Id)));

    public IEnumerable<Field> StrongestFieldNear(Field field) => OwnFields
        .Where(x => _geometry.AreNeighboringFields(x.Id, field.Id))
        .OrderByDescending(x => x.DiceCount);

    public IEnumerable<Field> VulnerableFields
    {
        get
        {
            var currentFieldSize = _geometry.GetLargestContinuousFieldBlock(ActivePlayerId);

            return OwnFields
                .Except(SafeFields)
                .OrderByDescending(field =>
                    new FieldGeometry(OwnFields.Except(new[] { field })).GetLargestContinuousFieldBlock(ActivePlayerId) / currentFieldSize);
        }
    }

    public IEnumerable<Field> MostAdvantagousAttackableNeighboringEnemyFields
    {
        get
        {
            var currentFieldSize = _geometry.GetLargestContinuousFieldBlock(ActivePlayerId);

            return AttackableNeighboringEnemyFields
                .OrderByDescending(field =>
                    new FieldGeometry(OwnFields.Append(field.Copy(ActivePlayerId)))
                        .GetLargestContinuousFieldBlock(ActivePlayerId) / currentFieldSize);
        }
    }
}
