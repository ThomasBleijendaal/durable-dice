using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Bots;

internal class NullBot : IBot
{
    public AttackMoveCommand? MakeMove() => null;
}
