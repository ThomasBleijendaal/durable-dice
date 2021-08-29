using DurableDice.Data.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableDice.Functions.EntityFunctions;

public static class GameEntityFunction
{
    [FunctionName(nameof(GameEntity))]
    public static Task ProxyGameEntityFunction(
        [EntityTrigger] IDurableEntityContext context) 
            => context.DispatchAsync<GameEntity>();
}
