using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Bots;

internal class StrategicBot : IBot
{
    private readonly BotState _botState;

    public StrategicBot(
        BotState botState)
    {
        _botState = botState;
    }

    public AttackMoveCommand? MakeMove()
    {
        var mostEffectiveAttack = _botState.MostAdvantagousAttackableNeighboringEnemyFields
            .FirstOrDefault(x => _botState.StrongestFieldNear(x).Any(y => x.DiceCount < y.DiceCount));

        if (mostEffectiveAttack != null && _botState.StrongestFieldNear(mostEffectiveAttack).FirstOrDefault() is Field strongestField)
        {
            return new AttackMoveCommand(_botState.PlayerId, strongestField.Id, mostEffectiveAttack.Id);
        }

        return null;
    }
}
