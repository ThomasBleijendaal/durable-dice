﻿using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Abstractions;

public interface IBot
{
    MoveCommand? MakeMove();
}
