using DurableDice.Common.Enums;

namespace DurableDice.Common.Models.Commands;

public record AddBotCommand(string PlayerId, BotType BotType);
