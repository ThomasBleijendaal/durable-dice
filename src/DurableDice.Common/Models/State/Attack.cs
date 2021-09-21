namespace DurableDice.Common.Models.State;

public class Attack
{
    public string AttackerId { get; set; } = "";
    public string AttackingFieldId { get; set; } = "";
    public List<int> AttackingDiceCount { get; set; } = new List<int>();
    public string DefenderId { get; set; } = "";
    public string DefendingFieldId { get; set; } = "";
    public List<int> DefendingDiceCount { get; set; } = new List<int>();
    public bool IsSuccessful { get; set; }
}
