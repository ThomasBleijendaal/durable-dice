﻿using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Bots;

internal class StrategicBot : IBot
{
    private readonly BotInsight _botInsight;

    public StrategicBot(
        BotInsight botInsight)
    {
        _botInsight = botInsight;
    }

    public MoveCommand? MakeMove()
    {
        if (_botInsight.GameState.ActivePlayer.DiceMovesThisTurn == 0)
        {
            var mostEffectiveAttack = _botInsight.MostAdvantagousAttackableNeighboringEnemyFields
                .FirstOrDefault(x => _botInsight.StrongestAlliedFieldNear(x).Any(y => x.DiceCount <= y.DiceCount));

            if (mostEffectiveAttack != null && _botInsight.StrongestAlliedFieldNear(mostEffectiveAttack).FirstOrDefault() is Field strongestField)
            {
                return new MoveCommand(_botInsight.ActivePlayerId, strongestField.Id, mostEffectiveAttack.Id);
            }
        }

        if (_botInsight.GameState.ActivePlayer.DiceMovesThisTurn < _botInsight.GameState.Rules.MaxDiceMovedPerTurn)
        {
            var vulnerableFields = _botInsight.VulnerableFields.ToList();

            var mostImprovingMove = vulnerableFields
                .Where(x => x.DiceCount < 4)
                .SelectMany(protect => _botInsight.StrongestAlliedFieldNear(protect).Where(donor => donor.DiceCount > 1).Select(donor => (protect, donor)))
                .OrderByDescending(combo => vulnerableFields.IndexOf(combo.protect) - vulnerableFields.IndexOf(combo.donor))
                .FirstOrDefault();

            if (mostImprovingMove != default)
            {
                return new MoveCommand(_botInsight.ActivePlayerId, mostImprovingMove.donor.Id, mostImprovingMove.protect.Id);
            }
        }

        return null;
    }
}
