using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class GameState
{
    [JsonProperty]
    public string? NextGameId { get; set; }

    [JsonProperty]
    public List<Player> Players { get; set; } = new List<Player>();

    [JsonProperty]
    public List<Field> Fields { get; set; } = new List<Field>();

    [JsonProperty]
    public string? ActivePlayerId { get; set; }

    [JsonProperty]
    public int GameRound { get; set; }

    [JsonProperty]
    public Attack? PreviousAttack { get; set; }

    [JsonProperty]
    public Move? PreviousMove { get; set; }

    [JsonProperty]
    public GameRules Rules { get; set; } = new GameRules();

    public Player ActivePlayer
        => Players.First(x => x.Id == ActivePlayerId);

    public Player? Winner
        => Fields.GroupBy(x => x.OwnerId).Count() == 1
            ? Players.First(x => x.Id == Fields.First().OwnerId)
            : null;

    public IEnumerable<Field> ActivePlayerFields
        => Fields.Where(x => x.OwnerId == ActivePlayerId);

    public IEnumerable<Field> PlayerRandomFields(string playerId)
        => Fields.Where(x => x.OwnerId == playerId).OrderBy(x => Guid.NewGuid());

    public int PlayerFieldCount(Player player) 
        => Fields.Count(x => x.OwnerId == player.Id);

    public bool PlayerIsDead(string playerId)
        => !Fields.Any(x => x.OwnerId == playerId);

    public bool PlayerIsOwner(string playerId)
        => Players.ElementAtOrDefault(0)?.Id == playerId;

    public Player? Player(string playerId)
        => Players.FirstOrDefault(x => x.Id == playerId);

    public int PlayerIndex(string playerId)
        => Players.FindIndex(x => x.Id == playerId);

    public int? WinnerIndex
        => Winner != null ? PlayerIndex(Winner.Id) : default(int?);
}
