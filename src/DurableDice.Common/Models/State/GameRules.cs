using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DurableDice.Common.Models.State;

[JsonObject(MemberSerialization.OptIn)]
public class GameRules
{
    [JsonProperty]
    [Required]
    public int StartDiceCountPerField { get; set; } = 2;

    [JsonProperty]
    [Required]
    public int InitialDiceBuffer { get; set; } = 0;

    [JsonProperty]
    [Required]
    public double DiceGenerationMultiplier { get; set; } = 1.0;

    [JsonProperty]
    [Required]
    public double DeadPlayerMultiplier { get; set; } = 0.0;

    public void EnsureValidRules()
    {
        StartDiceCountPerField = Math.Max(1, Math.Min(8, StartDiceCountPerField));
        InitialDiceBuffer = Math.Max(0, Math.Min(50, InitialDiceBuffer));
        DiceGenerationMultiplier = Math.Max(1.0, Math.Min(10.0, InitialDiceBuffer));
        DeadPlayerMultiplier = Math.Max(0.0, Math.Min(2.0, DeadPlayerMultiplier));
    }
}
