using DurableDice.Common.Abstractions;
using DurableDice.Common.Bots;
using DurableDice.Common.Enums;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Helpers;

public static class BotHelper
{
    public static IBot BuildBot(GameState gameState)
    {
        var insight = new BotInsight(gameState, gameState.Fields);

        return gameState.ActivePlayer.BotType switch
        {
            BotType.CheezyBot => new CheezyBot(insight),
            BotType.StrategicBot => new StrategicBot(insight),

            _ => new NullBot()
        };
    }

    public static string BotName(BotType type, int botIndex)
        => type switch
        {
            BotType.CheezyBot => new[]
            {
                "l33t",
                "h4x",
                "ch3z3",
                "3z3",
                "gg no re",
                "2ez",
                "usux",
                "h4x0r"
            }[botIndex],
            BotType.StrategicBot => new[]
            {
                "Butcher",
                "Blaster",
                "Cannon",
                "Destroyer",
                "Nuke",
                "Monster",
                "Terminator",
                "Zealot",

            }[botIndex],

            _ => "BOT"
        };
}
