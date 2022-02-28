using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodierBot.Services
{
  public class CommandHandler
  {
    private readonly IConfiguration _config;
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    // Constructor
    public CommandHandler(IServiceProvider services)
    {
      _config = services.GetRequiredService<IConfiguration>();
      _commands = services.GetRequiredService<CommandService>();
      _client = services.GetRequiredService<DiscordSocketClient>();
      _logger = services.GetRequiredService<ILogger<CommandHandler>>();
      _services = services;

      _commands.CommandExecuted += CommandExecutedAsync;
      _client.MessageReceived += MessageReceivedAsync;
    }

    // Initialization
    public async Task InitializeAsync()
    {
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }


    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
      // is system or bot message?
      if (!(rawMessage is SocketUserMessage message))
      {
        return;
      }

      // is user message?
      if (message.Source != MessageSource.User)
      {
        return;
      }

      // Does the message start with a valid prefix? Adjust argPos 
      var argPos = 0;
      char prefix = Char.Parse(_config["Prefix"]);
      if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix,ref argPos)))
      {
        return;
      }

      // Get Context
      var context = new SocketCommandContext(_client, message);

      // Execute command
      await _commands.ExecuteAsync(context, argPos, _services);
    }


    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
      // Command was not found
      if (!command.IsSpecified)
      {
        _logger.LogError($"Command failed to execute for [] <-> []!");
        return;
      }

      // Command was Successfully executed
      if (result.IsSuccess)
      {
        _logger.LogInformation($"Command [{((CommandInfo)command).Name}] executed for -> [{context.User}] on []");
        return;
      }

      if (!result.IsSuccess && result.Error != null)
      {
        await context.Channel.SendMessageAsync($"{result.ErrorReason}");
        return;
      }
      // Command failed to execute
      await context.Channel.SendMessageAsync($"Sorry, ... something went wrong -> []!");
    }
  }
}
