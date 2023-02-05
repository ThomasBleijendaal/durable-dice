using DurableDice.Common.Enums;
using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class Player
{
    [JsonProperty]
    public int Index { get; set; }

    [JsonProperty]
    public string Id { get; set; } = "";

    [JsonProperty]
    public string Name { get; set; } = "";

    [JsonProperty]
    public int DiceBuffer { get; set; }

    [JsonProperty]
    public int DiceMovesThisTurn { get; set; }

    [JsonProperty]
    public int ContinuousFieldCount { get; set; }

    [JsonProperty]
    public bool IsReady { get; set; }

    [JsonProperty]
    public BotType? BotType { get; set; }
}
