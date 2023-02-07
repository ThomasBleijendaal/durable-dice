using DurableDice.Common.Geometry;

namespace DurableDice.Common.Models.State;

internal class BotInsight
{
    public BotInsight(GameState gameState)
    {
        GameState = gameState;
    }

    public string ActivePlayerId => GameState.ActivePlayerId ?? throw new ArgumentException("Invalid game state");

    public GameState GameState { get; }

    public IEnumerable<Player> Enemies
    {
        get
        {
            var enemies = GameState.Players.Where(x => x.Id != GameState.ActivePlayerId);
            var biggestEnemy = enemies.MaxBy(x => x.ContinuousFieldCount);
            if (biggestEnemy?.ContinuousFieldCount > 10)
            {
                var bigTargets = enemies.Where(x => (x.ContinuousFieldCount / biggestEnemy.ContinuousFieldCount) > 0.7);

                return bigTargets;
            }

            return enemies;
        }
    }

    public IReadOnlyList<Field> Fields => GameState.Fields;

    public IEnumerable<Field> EnemyFields => Enemies.SelectMany(e => Fields.Where(f => e.Id == f.OwnerId));

    public IEnumerable<Field> OwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId);

    public IEnumerable<Field> AttackingOwnFields => Fields.Where(x => x.OwnerId == ActivePlayerId && x.DiceCount > 1);

    public IEnumerable<Field> AttackableNeighboringEnemyFields => EnemyFields
        .Where(x => AttackingOwnFields.Any(o => o.IsNeighbor(x) && o.DiceCount >= x.DiceCount));

    public int DiceBeyondField(Field field) => StrongestEnemyFieldNear(field).Sum(x => x.DiceCount);

    public IEnumerable<Field> NeighboringEnemyFields => EnemyFields
        .Where(x => OwnFields.Any(o => o.IsNeighbor(x)));

    public IEnumerable<Field> SafeFields => OwnFields
        .Where(field => NeighboringEnemyFields.All(n => !field.IsNeighbor(n) || n.DiceCount == 1));

    public IEnumerable<Field> StrongestEnemyFieldNear(Field field) => Fields
        .Where(x => x.OwnerId != ActivePlayerId && field.IsNeighbor(x))
        .OrderByDescending(x => x.DiceCount);

    public IEnumerable<Field> NeighborsOf(Field field) => OwnFields.Where(o => field.Neighbors.Contains(o.Index));

    public IEnumerable<Field> StrongestAlliedFieldNear(Field field) => OwnFields
        .Where(x => field.IsNeighbor(x))
        .OrderByDescending(x => x.DiceCount);

    public IEnumerable<Field> VulnerableFields
    {
        get
        {
            var currentFieldSize = FieldGeometry.GetLargestContinuousFieldBlock(Fields, ActivePlayerId);

            var order = OwnFields
                .Except(SafeFields)
                .Where(field => field.MaxDiceAllowedToAdd > 0)
                .Where(field => StrongestAlliedFieldNear(field).Any(x => x.DiceCount > 1))
                .Select(field => (field, sizeAfterLossingThis: FieldGeometry.GetLargestContinuousFieldBlock(OwnFields.Except(new[] { field }))))
                .Where(x => x.sizeAfterLossingThis < currentFieldSize - 1)
                .Where(x => ((double)x.sizeAfterLossingThis / currentFieldSize) < 0.8)
                .OrderBy(x => (double)x.sizeAfterLossingThis / currentFieldSize)
                .ToList();

            return order.Select(x => x.field);
        }
    }

    public IEnumerable<Field> MostAdvantagousAttackableNeighboringEnemyFields
    {
        get
        {
            var currentFieldSize = FieldGeometry.GetLargestContinuousFieldBlock(Fields, ActivePlayerId);

            return AttackableNeighboringEnemyFields
                .OrderBy(field => (double)FieldGeometry.GetLargestContinuousFieldBlock(OwnFields.Append(field.Copy(ActivePlayerId))) / currentFieldSize);
        }
    }

    public IEnumerable<Field> MostAdvantagousNeighboringEnemyFields
    {
        get
        {
            var currentFieldSize = FieldGeometry.GetLargestContinuousFieldBlock(Fields, ActivePlayerId);

            return NeighboringEnemyFields
                .OrderBy(field => (double)FieldGeometry.GetLargestContinuousFieldBlock(OwnFields.Append(field.Copy(ActivePlayerId))) / currentFieldSize);
        }
    }

    public IEnumerable<Field> MostDestructiveAttackableNeighboringEnemyFields
    {
        get
        {
            var fields = AttackableNeighboringEnemyFields.Select(field =>
                {
                    var ownerSize = GameState.Player(field.OwnerId)?.ContinuousFieldCount ?? 0;

                    if (ownerSize == 0)
                    {
                        return (field, false, 0);
                    }

                    var ownerSizeAfterLosingThis = FieldGeometry.GetLargestContinuousFieldBlock(Fields.Except(new[] { field }), field.OwnerId);

                    return (field, losesMoreThanOne: ownerSizeAfterLosingThis + 1 < ownerSize, ratio: (double)ownerSizeAfterLosingThis / ownerSize);
                })
                .Where(x => x.losesMoreThanOne)
                .OrderBy(x => x.ratio)
                .ToList();

            return fields.Select(x => x.field);
        }
    }
}
