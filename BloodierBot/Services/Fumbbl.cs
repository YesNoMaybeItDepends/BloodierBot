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

    /// <summary>
    /// Inserts a RunningGame + Tournament (if applicable) + Teams.
    /// <para />
    /// Does NOT dispose Database Connection
    /// </summary>
    /// <param name="game"></param>
    /// <param name="db"></param>
    public void InsertRunningGame(RunningGame game, IDbConnection db)
    {
      // Add new games
      string query1 = "INSERT or IGNORE INTO RunningGames (Id, Half, Turn, Division, RunningGameTournamentId) values (@Id, @Half, @Turn, @Division, @RunningGameTournamentId)";
      DynamicParameters params1 = new DynamicParameters();
      params1.Add("Id", game.RunningGame_Id);
      params1.Add("Half", game.half);
      params1.Add("Turn", game.turn);
      params1.Add("Division", game.division);
      if (game.tournament != null)
      {
        params1.Add("RunningGameTournamentId", game.tournament.RunningGameTournament_Id);
      }
      else
      {
        params1.Add("RunningGameTournamentId", null);
      }
      db.Execute(query1, params1);
      // Ad new tournaments
      if (game.tournament != null)
      {
        string query2 = "INSERT or IGNORE INTO RunningGameTournaments (Id, GroupId) values (@Id, @GroupId)";
        var params2 = new DynamicParameters();
        params2.Add("Id", game.tournament.RunningGameTournament_Id);
        params2.Add("GroupId", game.tournament.group);
        db.Execute(query2, params2);
      }
      // Ad new teams
      foreach (var team in game.teams)
      {
        string query3 = "INSERT or IGNORE INTO RunningGameTeams (Id, RunningGameID, Side, Name, Coach, Race, Tv, Rating, Score, Logo, LogoLarge) values (@Id, @RunningGameID, @Side, @Name, @Coach, @Race, @Tv, @Rating, @Score, @Logo, @LogoLarge)";
        DynamicParameters params3 = new DynamicParameters();
        params3.Add("Id", team.RunningGameTeam_Id);
        params3.Add("RunningGameId", game.RunningGame_Id);
        params3.Add("Side", team.side);
        params3.Add("Name", team.name);
        params3.Add("Coach", team.coach);
        params3.Add("Race", team.race);
        params3.Add("Tv", team.tv);
        params3.Add("Rating", team.rating);
        params3.Add("Score", team.score);
        params3.Add("Logo", team.logo);
        params3.Add("LogoLarge", team.logolarge);
        db.Execute(query3, params3);
      }
    }

    // TODO this method is redundant, theres already a getrunningames and an insertrunninggame method
    /// <summary>
    /// Gets all RunningGames + Tournament (if applicable) + Teams.
    /// </summary>
    /// <param name="db"></param>
    [Obsolete("this method is redundant, theres already a getrunningames and an insertrunninggame method")]
    public async void GetRunningGames_DEPRECATED(IDbConnection db)
    {
      var livegames = await _fumbbl.GetRunningGames();

      foreach(var game in livegames)
      {
        InsertRunningGame(game, db);
      }
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
            var dbgames = GetRunningGamesFromDatabase();

            
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
              if (game.teams.Count() == 2)
              {
              sb.AppendLine($"{game.teams[0]?.name} vs {game.teams[1]?.name} / {game.tournament?.group}");
              }
              else
              {
                // TODO !! BUG !! Game only has 1 team on db
                sb.AppendLine($"Error on game with ID {game.RunningGame_Id}, it only has 1 team in the db");
              }
              await ResolveGame(game);
              DeleteRunningGame(db, game.RunningGame_Id);
            }

            sb.AppendLine("**NEW GAMES**");
            foreach (var game in gamesToAnnounce)
            {
              sb.AppendLine($"{game.teams[0].name} vs {game.teams[1].name} / {game.tournament?.group}");
              InsertRunningGame(game, db);
              await AnnounceGame(game);
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

    public List<RunningGame> GetRunningGamesFromDatabase()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        //Fumbbl fb = new Fumbbl();
        //fb.GetRunningGames(db);
        var gamesById = new Dictionary<int, RunningGame>();
        db.Query<RunningGame, RunningGameTournament, RunningGameTeam, RunningGame>(Properties.Resources.GetRunningGamesFromDatabase,
          (g, tt, t) =>
          {
            RunningGame game;
            if (!gamesById.TryGetValue(g.RunningGame_Id, out game))
            {
              gamesById.Add(g.RunningGame_Id, game = g);
            }
            if (game.teams == null)
            {
              game.teams = new List<RunningGameTeam>();
            }
            game.teams.Add(t);
            if (tt != null)
            {
              game.tournament = tt;
            }
            return game;
          }, splitOn: "RunningGame_Id,RunningGameTournament_Id,RunningGameTeam_Id").AsQueryable();
        
        return gamesById.Values.ToList();
      }
    }

    public void DeleteRunningGame(IDbConnection db, int runningGameId)
    {
      DynamicParameters args = new DynamicParameters();
      args.Add("RunningGameId", runningGameId);

      var deletedTeams = db.Execute(Properties.Resources.DeleteRunningGameTeams, args);

      var deletedGames = db.Execute(Properties.Resources.DeleteRunningGame, args);
      Console.WriteLine("Deleted Teams:"+deletedTeams);
      Console.WriteLine("Deleted Games:" + deletedGames);
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

    public async Task ResolveGame(RunningGame game)
    {
      var meme = await FindRecentMatchFromRunningGame(game);
      Console.WriteLine(meme.team1.score + "-" + meme.team2.score);
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
  }
}
