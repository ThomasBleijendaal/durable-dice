using DurableDice.Common.Enums;
using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class GameState
{
    [JsonProperty]
    public List<Player> Players { get; set; } = new List<Player>();

    [JsonProperty]
    public List<Field> Fields { get; set; } = new List<Field>();

    [JsonProperty]
    public string? ActivePlayerId { get; set; }

    [JsonProperty]
    public string? OwnerId { get; set; }

    [JsonProperty]
    public MatchState State { get; set; }
}
