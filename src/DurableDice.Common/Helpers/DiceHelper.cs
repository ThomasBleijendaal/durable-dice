using System.Security.Cryptography;
using DurableDice.Common.Abstractions;
using DurableDice.Common.Bots;
using DurableDice.Common.Enums;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Helpers;

public static class DiceHelper
{
    public static IEnumerable<int> ThrowDice(int numberOfDice)
        => Enumerable.Range(0, numberOfDice)
            .Select(i => RandomNumberGenerator.GetInt32(6) + 1);
}

public static class BotHelper
{
    public static IBot BuidBot(GameState gameState, Player activePlayer)
    {
        var state = new BotState(activePlayer.Id, gameState.Fields);

        return activePlayer.BotType switch
        {
            BotType.Noob => new NoobBot(state),

            _ => new NullBot()
        };
    }
}
