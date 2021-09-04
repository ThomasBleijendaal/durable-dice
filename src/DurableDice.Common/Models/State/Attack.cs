namespace DurableDice.Common.Models.State;

public class Attack
{
    public string AttackerId { get; set; } = "";
    public int AttackingDiceCount { get; set; }
    public string DefenderId { get; set; } = "";
    public int DefendingDiceCount { get; set; }
}
