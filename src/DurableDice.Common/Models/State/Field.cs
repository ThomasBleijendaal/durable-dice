namespace DurableDice.Common.Models.State;

public class Field
{
    public string Id { get; set; } = "";

    public string OwnerId { get; set; } = "";

    public int DiceCount { get; set; }
}
