using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Bots;

internal class CheezyBot : IBot
{
    private readonly BotInsight _botInsight;

    public CheezyBot(
        BotInsight botInsight)
    {
        _botInsight = botInsight;
    }

    public MoveCommand? MakeMove()
    {
        var randomEnemyField = _botInsight.AttackableNeighboringEnemyFields.OrderBy(x => x.DiceCount).FirstOrDefault();
        if (randomEnemyField == null)
        {
            return null;
        }

        var strongestField = _botInsight.StrongestAlliedFieldNear(randomEnemyField).FirstOrDefault();
        if (strongestField == null)
        {
            return null;
        }

        if (randomEnemyField.DiceCount > strongestField.DiceCount)
        {
            return null;
        }
        
        return new MoveCommand(_botInsight.ActivePlayerId, strongestField.Id, randomEnemyField.Id);
    }
}
