using BloodierBot.Database;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Dapper;
using BloodierBot.Database.Models;
using Discord;

namespace BloodierBot.Services
{
  public class Fumbbl
  {
    private readonly IConfiguration _config;
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly FumbblApi _fumbbl;
    //private readonly BloodierBotContext _db;

    // Constructor
    public Fumbbl(IServiceProvider services)
    {
      _config = services.GetRequiredService<IConfiguration>();
      _commands = services.GetRequiredService<CommandService>();
      _client = services.GetRequiredService<DiscordSocketClient>();
      _logger = services.GetRequiredService<ILogger<CommandHandler>>();
      _fumbbl = services.GetRequiredService<FumbblApi>();
      //_db = services.GetRequiredService<BloodierBotContext>();
      _services = services;
    }

    // Initialization
    // TODO Should be on client ready async
    public async Task InitializeAsync()
    {
      // TODO what does this do?
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    // TODO test this
    /// <summary>
    /// Inserts a ScheduledMatch + TODO also insert winner
    /// <para />
    /// Does NOT dispose Database Connection
    /// </summary>
    /// <param name="match"></param>
    /// <param name="db"></param>
    public void InsertScheduledMatch(ScheduledMatch match, IDbConnection db)
    {
      DynamicParameters params1 = new DynamicParameters();
      
      // Add basic parameters
      params1.Add("Id", match.Id);
      params1.Add("TournamentId", match.tournamentId);
      params1.Add("Position", match.position);
      params1.Add("TRound", match.round);
      params1.Add("Created", match.created);
      
      // Add modified parameter    
      if (match.modified != null)
      {
        params1.Add("Modified", match.modified);
      }
      else
      {
        params1.Add("Modified", null);
      }
      
      // Add Result parameter
      if (match.result != null)
      {
        params1.Add("ResultId", match.result.id);
      }
      else
      {
        params1.Add("ResultId", null);
      }
      
      // Add Team parameters
      params1.Add("ATeamId", match.teams[0].id);
      params1.Add("BTeamId", match.teams[1].id);

      // Add result
      db.Execute(Properties.Resources.insertScheduledMatch, params1);
    }

    public async Task run()
    {
      while (true)
      {
        StringBuilder sb = new StringBuilder();
        string newRunCycleString = "["+DateTime.UtcNow.AddHours(1).ToLongTimeString() + " DBG] : New run() cycle";
        sb.AppendLine(newRunCycleString);
        Console.WriteLine(newRunCycleString);
        // Get CurrentGames from the DB
        // Compare
        // Find games to announce
        // Find games to resolve
        try
        {
          using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
          {
            var livegames = await _fumbbl.GetRunningGames();
            var dbgames = RunningGame.GetRunningGamesFromDatabase(db);

            var gamesToAnnounce = livegames.Except(dbgames).ToList();
            var gamesToResolve = dbgames.Except(livegames).ToList();

            // Resolving games
            foreach (var game in gamesToResolve)
            {
              sb.AppendLine($"{game.teams[0]?.name} vs {game.teams[1]?.name} / {game.tournament?.group}");
              // If we fail to resolve the game, try again in the next run
              if (await ResolveGame(game) == true)
              {
                game.DeleteRunningGame(db);
              }
            }
            
            // New games
            List<Tournament> trackedTournaments = await Tournament.DbSelectAllTournaments(db);
            var trackedTournamentIds = trackedTournaments.Select(x => x.Tournament_Id).ToList();
            
            foreach (var game in gamesToAnnounce)
            {
              if (game.tournament != null && trackedTournamentIds.Contains(game.tournament.Tournament_Id))
              {
                sb.AppendLine($"{game.teams[0].name} vs {game.teams[1].name} / {game.tournament?.group}");
                await game.DbInsertRunningGame(db);
                await AnnounceGame(game);
              }
            }
          }
          ulong debugChannelId = ulong.Parse(_config["Channel_Debug"]);
          var debugChannel = _client.GetChannel(debugChannelId) as SocketTextChannel;
          await debugChannel.SendMessageAsync(sb.ToString());
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("["+DateTime.UtcNow.AddHours(1).ToLongTimeString()+" DBG] : Sleeping...");
        Thread.Sleep(60000);
      }
    }

    public async Task AnnounceGame(RunningGame game)
    {
      var eb = new EmbedBuilder();

      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      //eb.WithColor(Discord.Color.DarkPurple);
      eb.WithColor(Color.Blue);
      eb.WithTitle("Spectate Game");
      eb.WithUrl($"https://fumbbl.com/ffblive.jnlp?spectate={game.RunningGame_Id}");

      // Fields
      var header = new EmbedFieldBuilder();
      header.WithName(":eye::eye:");
      header.WithValue($"**{game.teams[0].name}** vs **{game.teams[1].name}**");
      var team1 = new EmbedFieldBuilder();
      team1.WithIsInline(true);
      team1.Name = $"{game.teams[0].coach}";
      team1.Value = $"{game.teams[0].race}\n{game.teams[0].tv}";
      var team2 = new EmbedFieldBuilder();
      team2.WithIsInline(true);
      team2.Name = $"{game.teams[1].coach}";
      team2.Value = $"{game.teams[1].race}\n{game.teams[1].tv}";

      eb.AddField(header);
      eb.AddField(team1);
      eb.AddField(team2);

      ulong channelId = ulong.Parse(_config["Channel_Announce"]);
      var channel = _client.GetChannel(channelId) as SocketTextChannel;


      await channel.SendMessageAsync(MentionUtils.MentionRole(326485871639658496),embed: eb.Build());

    }

    public async Task<bool> ResolveGame(RunningGame livegame)
    {
      ulong resolvingId = ulong.Parse(_config["Channel_Resolve"]);
      ulong errorsId = ulong.Parse(_config["Channel_Error"]);
      var channelResolving = _client.GetChannel(resolvingId) as SocketTextChannel;
      var channelErrors = _client.GetChannel(errorsId) as SocketTextChannel;
      var game = await FindRecentMatchFromRunningGame(livegame);
      
      if (game != null)
      {
        await channelResolving.SendMessageAsync(embed: EmbedRecentMatch(game));
        return true;
      }
      else
      {
        // retry
        await channelErrors.SendMessageAsync("@ItDepends WARNING ERROR OH GOD"+livegame.teams[0]+"/"+ livegame.teams[1]);
        return false;
      }
    }

    public async Task<RecentMatch> FindRecentMatchFromRunningGame(RunningGame game)
    {
      Console.WriteLine("Finding recent match from running games");

      RecentMatch recentGame = null;

      var matchHistory = await _fumbbl.GetTeamMatches(game.teams[0].RunningGameTeam_Id);
      if (matchHistory != null)
      {
        Console.WriteLine("Obtained home team match history");
      }
      else
      {
        Console.WriteLine("Failed to obtain home tema match history");
      }

      var homeTeamName = game.teams[0].name;
      var homeTeamTV = game.teams[0].tv;
      Console.WriteLine("Home team: "+homeTeamName);

      var awayTeamName = game.teams[1].name;
      var awayTeamTV = game.teams[1].tv;
      Console.WriteLine("Away team: "+awayTeamName);

      if (matchHistory != null)
      {
        recentGame = matchHistory.Find(historyGame =>
      ((historyGame.team1.name == homeTeamName) || (historyGame.team1.name == awayTeamName))
      &&
      ((historyGame.team2.name == homeTeamName) || (historyGame.team2.name == awayTeamName)));
      }
      if (recentGame != null)
      {
        return recentGame;
      }
      else
      {
        Console.WriteLine("Could not find recentgame in home team's match history");
        ulong errorsId = ulong.Parse(_config["Channel_Error"]);
        var channelErrors = _client.GetChannel(errorsId) as SocketTextChannel;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Could not find recentgame in home team's match history");
        sb.AppendLine($"{homeTeamName} vs {awayTeamName}");
        sb.AppendLine("Half: " + game.half);
        sb.AppendLine("Turn: " + game.turn);
        await channelErrors.SendMessageAsync(sb.ToString());
        return recentGame;
      }
    }

    public Embed EmbedRecentMatch(RecentMatch game)
    {
      var eb = new EmbedBuilder();
      var team1 = new EmbedFieldBuilder();
      var vs = new EmbedFieldBuilder();
      var team2 = new EmbedFieldBuilder();
      var coach1 = new EmbedFieldBuilder();
      var misc = new EmbedFieldBuilder();
      var coach2 = new EmbedFieldBuilder();
      var footer = new EmbedFieldBuilder();

      // Team 1
      team1.WithIsInline(true);
      team1.WithName($"{game.team1.name}");
      team1.WithValue("\u200b");

      // Vs
      vs.WithIsInline(true);
      vs.WithName($"{game.team1.score} - {game.team2.score}");
      vs.WithValue("\u200b");

      // Team 2
      team2.WithIsInline(true);
      team2.WithName($"{game.team2.name}");
      team2.WithValue("\u200b");

      // Coach 1
      coach1.WithIsInline(true);
      coach1.WithName($"{game.team1.coach.name}");
      var coach1string = new StringBuilder();
      coach1string.AppendLine("*Race*");
      coach1string.AppendLine(game.team1.teamValue);
      coach1.WithValue($"{coach1string}");

      // Misc
      misc.WithIsInline(true);
      misc.WithName("\u200b");
      misc.WithValue("\u200b");

      // Coach 2
      coach2.WithIsInline(true);
      coach2.WithName($"{game.team2.coach.name}");
      var coach2string = new StringBuilder();
      coach2string.AppendLine("*Race*");
      coach2string.AppendLine(game.team2.teamValue);
      coach2.WithValue($"{coach2string}");

      // Footer
      footer.WithIsInline(false);
      footer.WithName("\u200b");
      footer.WithValue($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");

      // Embed
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.WithColor(Color.Green);
      eb.WithTitle("Match Result");
      eb.WithUrl($"https://fumbbl.com/p/match?id={game.RecentMatch_Id}");
      eb.WithDescription($"\n");
      //eb.WithDescription($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");
      eb.AddField(team1);
      eb.AddField(vs);
      eb.AddField(team2);
      eb.AddField(coach1);
      eb.AddField(misc);
      eb.AddField(coach2);
      eb.AddField(footer);
      //eb.WithFooter($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");

      return eb.Build();
    }
  }
}
