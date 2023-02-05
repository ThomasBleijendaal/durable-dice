using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class Field
{
    [JsonProperty]
    public int Index { get; set; }

    [JsonProperty]
    public string Id { get; set; } = "";

    [JsonProperty]
    public string OwnerId { get; set; } = "";

    [JsonProperty]
    public int DiceCount { get; set; }

    [JsonProperty]
    public int DiceAdded { get; set; }

    [JsonProperty]
    public List<Coordinate> Coordinates { get; set; } = new List<Coordinate>();

    [JsonProperty]
    public Coordinate Center { get; set; }

    [JsonProperty]
    public List<int> Neighbors { get; set; } = new List<int>();

    public int MaxDiceAllowedToAdd => 8 - DiceCount;

    public Field Copy(string ownerId) => new Field
    {
        Id = Id,
        OwnerId = ownerId,
        DiceCount = DiceCount,
        DiceAdded = DiceAdded,
        Coordinates = Coordinates,
        Center = Center
    };
}
