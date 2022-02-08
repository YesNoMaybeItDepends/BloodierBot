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
        
        try
        {
          // Get CurrentGames from the DB
          // Compare
          // Find games to announce
          // Find games to resolve

          EmbedBuilder eb = new EmbedBuilder();
          eb.WithTitle("lol");
          eb.WithDescription("lmao");
          eb.WithImageUrl("https://fumbbl.com/i/582020");

          using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
          {
            var livegames = await _fumbbl.GetRunningGames();
            var dbgames = RunningGame.GetRunningGamesFromDatabase(db);

            var gamesToAnnounce = livegames.Except(dbgames).ToList();
            var gamesToResolve = dbgames.Except(livegames).ToList();
            
            // DEBUG
            Console.WriteLine();
            Console.WriteLine("Games on fumbbl: " + livegames.Count());
            Console.WriteLine("Games on database: " + dbgames.Count());
            Console.WriteLine("Games to announce: " + gamesToAnnounce.Count());
            Console.WriteLine("Games to resolve: " + gamesToResolve.Count());
            Console.WriteLine();

            sb.AppendLine("**FINISHED GAMES**");
            foreach (var game in gamesToResolve)
            {
              sb.AppendLine($"{game.teams[0]?.name} vs {game.teams[1]?.name} / {game.tournament?.group}");
              await ResolveGame(game);
              game.DeleteRunningGame(db);
            }

            sb.AppendLine("**NEW GAMES**");
            List<Tournament> tournaments = await Tournament.DbSelectAllTournaments(db);
            var tournamentids = tournaments.Select(x => x.Tournament_Id).ToList();
            foreach (var game in gamesToAnnounce)
            {
              //if (game.tournament != null && tournamentids.Contains(game.tournament.Tournament_Id))
              {
                sb.AppendLine($"{game.teams[0].name} vs {game.teams[1].name} / {game.tournament?.group}");
                game.DbInsertRunningGame(db);
                await AnnounceGame(game);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        var meme = _client.GetChannel(309774261667627008) as SocketTextChannel;

        await meme.SendMessageAsync(sb.ToString());

        Console.WriteLine("finna sleep");
        Thread.Sleep(60000);
        Console.WriteLine("slept");
      }
    }

    public async Task AnnounceGame(RunningGame game)
    {
      var eb = new EmbedBuilder();

      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.WithColor(Discord.Color.DarkPurple);
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

      var meme = _client.GetChannel(309774261667627008) as SocketTextChannel;

      await meme.SendMessageAsync(embed: eb.Build());
    }

    public async Task ResolveGame(RunningGame livegame)
    {
      var channel = _client.GetChannel(309774261667627008) as SocketTextChannel;
      var game = await FindRecentMatchFromRunningGame(livegame);
      
      if (game != null)
      {
        await channel.SendMessageAsync(embed: EmbedRecentMatch(game));
      }
      else
      {
        await channel.SendMessageAsync("@ItDepends WARNING ERROR OH GOD"+livegame.teams[0]+"/"+ livegame.teams[1]);
      }
    }

    public async Task<RecentMatch> FindRecentMatchFromRunningGame(RunningGame game)
    {
      RecentMatch recentGame = null;
      
      var matchHistory = await _fumbbl.GetTeamMatches(game.teams[0].RunningGameTeam_Id);

      var homeTeamName = game.teams[0].name;
      var homeTeamTV = game.teams[0].tv;

      var awayTeamName = game.teams[1].name;
      var awayTeamTV = game.teams[1].tv;

      if (matchHistory != null)
      {
        recentGame = matchHistory.Find(historyGame =>
      ((historyGame.team1.name == homeTeamName) || (historyGame.team1.name == awayTeamName))
      &&
      ((historyGame.team2.name == homeTeamName) || (historyGame.team2.name == awayTeamName)));
      }
      return recentGame;
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
      eb.WithColor(Color.DarkPurple);
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
