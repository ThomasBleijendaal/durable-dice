using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class Player
{
    [JsonProperty]
    public string Id { get; set; } = "";

    [JsonProperty]
    public string Name { get; set; } = "";

    [JsonProperty]
    public int DiceBuffer { get; set; }

    [JsonProperty]
    public int ContinuousFieldCount { get; set; }

    [JsonProperty]
    public bool Ready { get; set; }
}
