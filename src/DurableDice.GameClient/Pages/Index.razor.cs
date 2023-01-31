using System.Text;
using Blazored.LocalStorage;
using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.History;
using DurableDice.Common.Models.State;
using DurableDice.GameClient.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace DurableDice.GameClient.Pages;

public partial class Index
{
    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Inject]
    public ISyncLocalStorageService LocalStorage { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter]
    public string? GameId { get; set; }

    private readonly string[] _colors = new []
    {
        "#ffc83d",
        "#e3008c",
        "#4f6bed",
        "#ca5010",
        "#00b294",
        "#498205",
        "#881798",
        "#986f0b"
    };

    private string ServerEndpoint => NavManager.BaseUri.Contains("localhost")
        ? "http://localhost:7071"
        : "https://durabledice.azurewebsites.net";

    private bool _connected = true;

    private string _playerId { get; set; } = Guid.NewGuid().ToString();
    private string _newPlayerName = "";
    private string _playerName = "";

    private string _fromFieldId = "";
    private string _toFieldId = "";
    private bool _attacking = false;

    private IGameEntity _gameEntity = null!;

    private GameState? _gameState;

    private List<GamePlayer>? _gameHistoryPlayers;
    private List<GameField>? _gameHistoryFields;

    private bool _sending = false;
    private string _buttonClassName => _sending ? "sending" : "";

    private IEnumerable<(Player player, int index)> _players
        => _gameState?.Players.Select((player, index) => (player, index)) ?? throw new Exception();

    private IEnumerable<(Field field, int index)> _fields
        => _gameState?.Fields.Select((field, index) => (field, index)) ?? throw new Exception();

    protected override void OnInitialized()
    {
        var localStoragePlayerId = "";

        if (string.IsNullOrEmpty(GameId) || !Guid.TryParse(GameId, out _))
        {
            GameId = Guid.NewGuid().ToString();
            NavManager.NavigateTo($"/{GameId}", false);
        }
        else
        {
            localStoragePlayerId = LocalStorage.GetItem<string>($"playerId-{GameId}");

            if (string.IsNullOrWhiteSpace(localStoragePlayerId))
            {
                Console.WriteLine("Saving playerId");

                _playerId = Guid.NewGuid().ToString();
                LocalStorage.SetItem($"playerId-{GameId}", _playerId);
            }
            else
            {
                _playerId = localStoragePlayerId;
                _playerName = _playerId;
            }
        }

        Console.WriteLine($"Joined game '{GameId}' as player '{_playerId}' -- (local storage player '{localStoragePlayerId}')");

        var gameEntityService = new GameEntityService(GameId, _playerId, ServerEndpoint);

        gameEntityService.NewStateReceived += NewStateAsync;
        gameEntityService.ConnectionState += ConnectionState;

        _gameEntity = gameEntityService;
    }

    private async void NewStateAsync(GameState state)
    {
        _sending = false;
        _gameState = state;

        if (_attacking)
        {
            _attacking = false;
            _fromFieldId = "";
            _toFieldId = "";
        }

        if (_gameState.Winner != null)
        {
            await DownloadStatsAsync();
        }

        StateHasChanged();
    }

    private void ConnectionState(bool connected)
    {
        _connected = connected;
        StateHasChanged();
    }

    private async Task JoinAsync()
    {
        if (!string.IsNullOrWhiteSpace(_newPlayerName))
        {
            _sending = true;
            _playerName = _newPlayerName;

            await _gameEntity.AddPlayerAsync(new AddPlayerCommand(_playerId, _playerName));
        }
    }

    private async Task ReadyAsync()
    {
        _sending = true;

        if (_gameState?.PlayerIsOwner(_playerId) ?? false)
        {
            await _gameEntity.ReadyWithRulesAsync(new ReadyPlayerCommand(_playerId, _gameState.Rules));
        }
        else
        {
            await _gameEntity.ReadyAsync(_playerId);
        }
    }

    private async Task EndRoundAsync()
    {
        _sending = true;
        await _gameEntity.EndRoundAsync(_playerId);
    }

    private async Task FieldClickAsync(Field field)
    {
        if (_attacking || _gameState == null || _gameState.ActivePlayerId != _playerId)
        {
            return;
        }

        if (string.IsNullOrEmpty(_fromFieldId))
        {
            if (field.OwnerId == _playerId && field.DiceCount > 1)
            {
                _fromFieldId = field.Id;
            }
        }
        else if (field.Id == _fromFieldId)
        {
            _fromFieldId = "";
        }
        else if (field.OwnerId != _playerId && _gameState.Geometry.AreNeighboringFields(_fromFieldId, field.Id))
        {
            _toFieldId = field.Id;
            await _gameEntity.AttackFieldAsync(new AttackMoveCommand(_playerId, _fromFieldId, field.Id));
            _attacking = true;
        }

        StateHasChanged();
    }

    private (int left, int top) Position(Coordinate coordinate)
    {
        var left = coordinate.X % 2 == 1
            ? coordinate.X * 21
            : coordinate.X * 21;
        var top = coordinate.X % 2 == 1
            ? 12 + coordinate.Y * 24
            : coordinate.Y * 24;

        return (left, top);
    }

    private static string DescribeAttack(Attack attack)
        => (attack.AttackingDiceCount.Sum() - attack.DefendingDiceCount.Sum()) switch
        {
            < -15 => "got owned by",
            < -10 => "never stood a change against",
            < -5 => "underestimated",
            < 0 => "came short against",
            0 => "did not have enough for",
            < 5 => "barely won of",
            < 10 => "bullied",
            < 15 => "destroyed",
            _ => "obliterated",
        };

    private async Task DownloadStatsAsync()
    {
        var historyJson = await HttpClient.GetStringAsync($"{ServerEndpoint}/history/{GameId}");

        var history = JsonConvert.DeserializeObject<GameHistory>(historyJson);

        if (history != null && _gameState != null)
        {
            _gameHistoryPlayers = history.GetPlayers();
            _gameHistoryFields = history.GetFields();

            var data = history.GetCount();

            await JSRuntime.InvokeVoidAsync("chart.Show", new
            {
                labels = data.Select((x, index) => index),
                datasets = _gameState.Players.Select((p, index) => new
                {
                    label = $"{p.Name} dice count",
                    data = data.Select(x => x.DiceCount[index]),
                    borderColor = _colors[index],
                    backgroundColor = _colors[index],
                    borderWidth = 3,
                    pointRadius = 0
                }).Concat(_gameState.Players.Select((p, index) => new
                {
                    label = $"{p.Name} field count",
                    data = data.Select(x => x.FieldCount[index]),
                    borderColor = _colors[index],
                    backgroundColor = _colors[index],
                    borderWidth = 3,
                    pointRadius = 0
                }))
            });

        }
    }

    public void Dispose()
    {
        if (_gameEntity is GameEntityService service)
        {
            service.NewStateReceived -= NewStateAsync;
        }
    }
}
