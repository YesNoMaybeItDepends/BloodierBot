using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BloodierBot.Services
{
  public class LoggingService
  {
    private readonly ILogger _logger;
    private readonly DiscordSocketClient _discord;
    private readonly CommandService _commands;

    public LoggingService(IServiceProvider services)
    {
      _discord = services.GetRequiredService<DiscordSocketClient>();
      _commands = services.GetRequiredService<CommandService>();
      _logger = services.GetRequiredService<ILogger<LoggingService>>();

      _discord.Ready += OnReadyAsync;
      _discord.Log += OnLogAsync;
      _commands.Log += OnLogAsync;
    }

    public Task OnReadyAsync()
    {
      _logger.LogInformation($"Connected as -> [] :)");
      _logger.LogInformation($"We are on [] servers");
      return Task.CompletedTask;
    }

    public Task OnLogAsync(LogMessage msg)
    {
      string logText = $": {msg.Exception?.ToString() ?? msg.Message}";

      switch (msg.Severity.ToString())
      {
        case "Critical":
          {
            _logger.LogCritical(logText);
            break;
          }
        case "Warning":
          {
            _logger.LogWarning(logText);
            break;
          }
        case "Info":
          {
            _logger.LogInformation(logText);
            break;
          }
        case "Verbose":
          {
            _logger.LogInformation(logText);
            break;
          }
        case "Debug":
          {
            _logger.LogDebug(logText);
            break;
          }
        case "Error":
          {
            _logger.LogError(logText);
            break;
          }
      }

      return Task.CompletedTask;
    }
  }
}
