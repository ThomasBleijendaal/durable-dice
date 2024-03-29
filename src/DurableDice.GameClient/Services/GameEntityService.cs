﻿using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace DurableDice.GameClient.Services;

public class GameEntityService : IGameEntity
{
    private readonly HubConnection _connection;

    private readonly Task _init;

    public GameEntityService(string gameId, string playerId, string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(baseUrl, (options) =>
            {
                options.Headers.Add("x-playerid", playerId);
                options.Headers.Add("x-gameid", gameId);
            })
            .Build();

        _connection.On<GameState>("Broadcast", (newState) => NewStateReceived?.Invoke(newState));
        _connection.Reconnecting += Reconnecting;
        _connection.Reconnected += Reconnected;

        _init = InitAsync();
    }

    public event Action<GameState>? NewStateReceived;
    public event Action<bool>? ConnectionState;

    public async Task InitAsync()
    {
        await _connection.StartAsync();
        await _connection.SendAsync("JoinGame");
    }

    public async Task AddBotAsync(AddBotCommand command)
    {
        await _init;
        await _connection.SendAsync("AddBot", command);
    }

    public async Task AddPlayerAsync(AddPlayerCommand command)
    {
        await _init;
        await _connection.SendAsync("AddPlayer", command);
    }

    public async Task MoveFieldAsync(MoveCommand command)
    {
        await _init;
        await _connection.SendAsync("MoveField", command);
    }

    public async Task EndRoundAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("EndRound");
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("RemovePlayer", playerId);
    }

    public async Task ReadyAsync(string playerId)
    {
        await _init;
        await _connection.SendAsync("Ready");
    }

    public async Task ReadyWithRulesAsync(ReadyPlayerCommand command)
    {
        await _init;
        await _connection.SendAsync("ReadyWithRules", command);
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
}
