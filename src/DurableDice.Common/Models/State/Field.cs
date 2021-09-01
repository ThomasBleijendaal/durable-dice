using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class Field
{
    [JsonProperty]
    public string Id { get; set; } = "";

    [JsonProperty]
    public string OwnerId { get; set; } = "";

    [JsonProperty]
    public int DiceCount { get; set; }

    public int MaxDiceAllowedToAdd => 8 - DiceCount;
}
