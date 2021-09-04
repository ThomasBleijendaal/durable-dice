using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace DurableDice.GameClient.Services;

public class GameEntityService : IGameEntity
{
    private readonly HubConnection _connection;

    private readonly Task _init;
    private readonly string _gameId;

    public GameEntityService(string gameId)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7071/api")
            .Build();

        _connection.On<GameState>("Broadcast", (newState) => NewStateReceived?.Invoke(newState));
        _connection.Reconnecting += Reconnecting;
        _connection.Reconnected += Reconnected;

        _init = InitAsync();
        _gameId = gameId;
    }

    public event Action<GameState>? NewStateReceived;
    public event Action<bool>? ConnectionState;

    public async Task AddPlayerAsync(AddPlayerCommand command)
    {
        await _init;
        await _connection.SendAsync("AddPlayer", _gameId, command);
    }

    public async Task AttackFieldAsync(AttackMoveCommand command)
    {
        await _init;
        await _connection.SendAsync("AttackField", _gameId, command);
    }

    public async Task EndRoundAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("EndRound", _gameId, playerId);
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("RemovePlayer", _gameId, playerId);
    }

    public async Task ReadyAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("Ready", _gameId, playerId);
    }

    private Task Reconnecting(Exception? arg)
    {
        ConnectionState?.Invoke(false);
        return Task.CompletedTask;
    }

    private Task Reconnected(string? arg)
    {
        ConnectionState?.Invoke(true);
        return Task.CompletedTask;
    }

    private async Task InitAsync()
    {
        await _connection.StartAsync();
        await _connection.SendAsync("JoinGame", _gameId);
    }
}
