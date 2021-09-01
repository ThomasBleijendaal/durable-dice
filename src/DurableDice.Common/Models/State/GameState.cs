using DurableDice.Common.Enums;
using DurableDice.Common.Geometry;
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

    private FieldGeometry? _geomertry;

    public FieldGeometry Geometry
    {
        get => _geomertry ??= new FieldGeometry(Fields);
    }

    public Player ActivePlayer 
        => Players.First(x => x.Id == ActivePlayerId);

    public IEnumerable<Field> ActivePlayerFields
        => Fields.Where(x => x.OwnerId == ActivePlayerId);

    public IEnumerable<Field> PlayerRandomFields(string playerId)
        => Fields.Where(x => x.OwnerId == playerId).OrderBy(x => Guid.NewGuid());

    public int PlayerFieldCount(Player player) 
        => Fields.Count(x => x.OwnerId == player.Id);
}
