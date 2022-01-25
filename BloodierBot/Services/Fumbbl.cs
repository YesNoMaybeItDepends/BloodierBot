﻿using BloodierBot.Database;
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
      db.Execute(Properties.Resources.InsertScheduledMatch, params1);
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
