using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Bots;

internal class NullBot : IBot
{
    public MoveCommand? MakeMove() => null;
}
