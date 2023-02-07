using DurableDice.Common.Abstractions;
using DurableDice.Common.Bots;
using DurableDice.Common.Enums;
using DurableDice.Common.Extensions;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Helpers;

public static class BotHelper
{
    private static readonly string[] CheezyBotNames = new[]
    {
        "l33t",
        "h4x",
        "ch3z3",
        "3z3",
        "gg no re",
        "2ez",
        "usux",
        "h4x0r"
    }.Shuffle().ToArray();

    private static readonly string[] StrategicBotNames = new[]
    {
        "Butcher",
        "Blaster",
        "Cannon",
        "Destroyer",
        "Nuke",
        "Monster",
        "Terminator",
        "Zealot",
    }.Shuffle().ToArray();

    private static readonly string[] NerdBotNames = new[]
    {
        "Analyzer",
        "Disrupter",
        "Exploiter",
        "Expulsor",
        "Excrusiator",
        "Quant",
        "Calculator",
        "Retaliator",
    }.Shuffle().ToArray();

    public static IBot BuildBot(GameState gameState)
    {
        var insight = new BotInsight(gameState);

        return gameState.ActivePlayer.BotType switch
        {
            BotType.CheezyBot => new CheezyBot(insight),
            BotType.StrategicBot => new StrategicBot(insight),
            BotType.NerdBot => new NerdBot(insight),

            _ => new NullBot()
        };
    }

    public static string BotName(BotType type, int botIndex)
        => type switch
        {
            BotType.CheezyBot => CheezyBotNames[botIndex],
            BotType.StrategicBot => StrategicBotNames[botIndex],
            BotType.NerdBot => NerdBotNames[botIndex],

            _ => "BORK"
        };
}
