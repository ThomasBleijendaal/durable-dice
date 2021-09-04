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

    [JsonProperty]
    public List<Coordinate> Coordinates { get; set; } = new List<Coordinate>();

    [JsonProperty]
    public Coordinate Center { get; set; }

    public int MaxDiceAllowedToAdd => 8 - DiceCount;
}
