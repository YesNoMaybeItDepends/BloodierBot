/*
    Get Group Tournaments
    Get Tournament Schedule
    Get Live Games

https://docs.stillu.cc/guides/text_commands/intro.html
https://fumbbl.com/apidoc/#/match/get_match_get__matchId_
https://www.gngrninja.com/code/2020/7/15/c-discord-bot-adding-a-database
 */

using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using BloodierBot.Services;
using Serilog;
using Microsoft.Extensions.Logging;
using BloodierBot.Database;

// main
public partial class Program
{
  private DiscordSocketClient _client;
  private readonly IConfiguration _config;
  private static string _logLevel;

  public Program()
  {
    // Make config file 
    var _configBuilder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile(path: "config.json");

    _config = _configBuilder.Build();
  }

  static void Main(string[] args = null)
  {
    if (args.Count() != 0)
    {
      _logLevel = args[0];
    }

    Log.Logger = new LoggerConfiguration()
      .WriteTo.File("Logs/BloodierBot.log", rollingInterval: RollingInterval.Day)
      .WriteTo.Console()
      .CreateLogger();

    new Program().MainAsync().GetAwaiter().GetResult();
  }

  public async Task MainAsync()
  {
    // fumbbl stuff
    Fumbbl fumbbl = new Fumbbl();
    fumbbl.GetLiveGames();

    using (ServiceProvider services = ConfigureServices())
    {
      // client
      var client = services.GetRequiredService<DiscordSocketClient>();
      _client = client;

      // setup logging and the ready event
      services.GetRequiredService<LoggingService>();

      await client.LoginAsync(TokenType.Bot, _config["Token"]);
      await client.StartAsync();

      await services.GetRequiredService<CommandHandler>().InitializeAsync();

      //await Task.Delay(10000);
      await Task.Delay(-1);
    }
  }

  private ServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection()
      .AddSingleton(_config)
      .AddSingleton<DiscordSocketClient>()
      .AddSingleton<CommandService>()
      .AddSingleton<CommandHandler>()
      .AddSingleton<LoggingService>()
      .AddDbContext<BloodierBotEntities>()
      .AddLogging(configure => configure.AddSerilog());
      
    if (!string.IsNullOrEmpty(_logLevel))
    {
      switch (_logLevel.ToLower())
      {
        case "info":
          {
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
            break;
          }
        case "error":
          {
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Error);
            break;
          }
        case "debug":
          {
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
            break;
          }
        default:
          {
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Error);
            break;
          }
      }
    }

    var serviceProvider = services.BuildServiceProvider();
    return serviceProvider;
  }
}
