using DurableDice.Common.Abstractions;
using DurableDice.Common.Extensions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Bots;

internal class NoobBot : IBot
{
    private readonly BotState _botState;

    public NoobBot(
        BotState botState)
    {
        _botState = botState;
    }

    public AttackMoveCommand? MakeMove()
    {
        var randomEnemyField = _botState.AttackableNeighboringEnemyFields.OrderBy(x => x.DiceCount).FirstOrDefault();
        if (randomEnemyField == null)
        {
            return null;
        }

        var strongestField = _botState.StrongestFieldNear(randomEnemyField).FirstOrDefault();
        if (strongestField == null)
        {
            return null;
        }

        if (randomEnemyField.DiceCount > strongestField.DiceCount)
        {
            return null;
        }

        return new AttackMoveCommand(_botState.PlayerId, strongestField.Id, randomEnemyField.Id);
    }
}
