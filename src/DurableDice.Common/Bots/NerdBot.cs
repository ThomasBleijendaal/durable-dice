using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Bots;

internal class NerdBot : IBot
{
    private readonly BotInsight _botInsight;

    public NerdBot(
        BotInsight botInsight)
    {
        _botInsight = botInsight;
    }

    public MoveCommand? MakeMove()
    {
        if (_botInsight.GameState.ActivePlayer.DiceMovesThisTurn == 0)
        {
            var cheapestAttack = _botInsight.MostDestructiveAttackableNeighboringEnemyFields
            .FirstOrDefault(x => _botInsight.StrongestAlliedFieldNear(x).Any(y => y.DiceCount - x.DiceCount > 1));

            if (cheapestAttack != null && _botInsight.StrongestAlliedFieldNear(cheapestAttack).FirstOrDefault() is Field strongestField1)
            {
                return new MoveCommand(_botInsight.ActivePlayerId, strongestField1.Id, cheapestAttack.Id);
            }

            var mostEffectiveAttack = _botInsight.MostAdvantagousAttackableNeighboringEnemyFields
                .FirstOrDefault(x => _botInsight.StrongestAlliedFieldNear(x).Any(y => x.DiceCount <= y.DiceCount));

            if (mostEffectiveAttack != null && _botInsight.StrongestAlliedFieldNear(mostEffectiveAttack).FirstOrDefault() is Field strongestField2)
            {
                return new MoveCommand(_botInsight.ActivePlayerId, strongestField2.Id, mostEffectiveAttack.Id);
            }
        }

        if (_botInsight.GameState.ActivePlayer.DiceMovesThisTurn < _botInsight.GameState.Rules.MaxDiceMovedPerTurn)
        {
            var stackOptionsForCheapshot = _botInsight.MostAdvantagousNeighboringEnemyFields
                .Select(field => _botInsight.StrongestAlliedFieldNear(field).FirstOrDefault())
                .OfType<Field>()
                .Where(field => field.MaxDiceAllowedToAdd > 0)
                .Select(field => (target: field, supplyFields: _botInsight.NeighborsOf(field).Where(x => x.DiceCount > 1).OrderBy(x => x.DiceCount)))
                .FirstOrDefault(x => x.supplyFields.Any());

            if (stackOptionsForCheapshot != default)
            {
                return new MoveCommand(_botInsight.ActivePlayerId, stackOptionsForCheapshot.supplyFields.First().Id, stackOptionsForCheapshot.target.Id);
            }
        }

        return null;
    }
}
