using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BloodierBot.Database.Models;
using BloodierBot.Services;
using System.Data;
using System.Data.SQLite;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using Slapper;
using System.Threading.Tasks;
using Discord;

namespace Tests
{
  [TestClass]
  public class TestFumbbl
  {
    private IConfiguration _config;
    private IDiscordClient _client;
    private FumbblApi _fapi;

    [TestInitialize]
    public void setup()
    {
      Console.WriteLine("Setup");
      var _configBuilder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile(path: "config.json");
      _config = _configBuilder.Build();

      _fapi = new FumbblApi();
    }

    /// <summary>
    /// Calls the Fumbbl API for currently running games, then inserts all of them into the database
    /// </summary>
    /// <returns>nothing</returns>

    //[TestMethod]
    //public async Task TestFumbblInsertRunningGames()
    //{
    //  var _fumbbl = new FumbblApi();
    //  var livegames = await _fumbbl.GetRunningGames();

    //  using (var db = new SQLiteConnection(_config["ConnectionString"]))
    //  {
    //    foreach (var game in livegames)
    //    {
    //      InsertRunningGame(game, db);
    //    }
    //  }
    //}

    // TODO why was this not a test method? why isnt it named Test?
    //[TestMethod]
    //public void InsertRunningGame(RunningGame game, IDbConnection db)
    //{
    //  // Add new game
    //  string query1 = "INSERT or IGNORE INTO RunningGames (Id, Half, Turn, Division, RunningGameTournamentId) values (@Id, @Half, @Turn, @Division, @RunningGameTournamentId)";
    //  DynamicParameters params1 = new DynamicParameters();
    //  params1.Add("Id", game.RunningGame_Id);
    //  params1.Add("Half", game.half);
    //  params1.Add("Turn", game.turn);
    //  params1.Add("Division", game.division);
    //  if (game.tournament != null)
    //  {
    //    params1.Add("RunningGameTournamentId", game.tournament.RunningGameTournament_Id);
    //  }
    //  else
    //  {
    //    params1.Add("RunningGameTournamentId", null);
    //  }
    //  db.Execute(query1, params1);
    //  // Add new tournament
    //  if (game.tournament != null)
    //  {
    //    Console.WriteLine(game.tournament.RunningGameTournament_Id + "//" + game.tournament.group);
    //    string query2 = "INSERT or IGNORE INTO RunningGameTournaments (Id, GroupId) values (@Id, @GroupId)";
    //    var params2 = new DynamicParameters();
    //    params2.Add("Id", game.tournament.RunningGameTournament_Id);
    //    params2.Add("GroupId", game.tournament.group);
    //    db.Execute(query2, params2);
    //  }
    //  // Add new teams
    //  foreach (var team in game.teams)
    //  {
    //    string query3 = "INSERT or IGNORE INTO RunningGameTeams (Id, RunningGameID, Side, Name, Coach, Race, Tv, Rating, Score, Logo, LogoLarge) values (@Id, @RunningGameID, @Side, @Name, @Coach, @Race, @Tv, @Rating, @Score, @Logo, @LogoLarge)";
    //    DynamicParameters params3 = new DynamicParameters();
    //    params3.Add("Id", team.RunningGameTeam_Id);
    //    params3.Add("RunningGameId", game.RunningGame_Id);
    //    params3.Add("Side", team.side);
    //    params3.Add("Name", team.name);
    //    params3.Add("Coach", team.coach);
    //    params3.Add("Race", team.race);
    //    params3.Add("Tv", team.tv);
    //    params3.Add("Rating", team.rating);
    //    params3.Add("Score", team.score);
    //    params3.Add("Logo", team.logo);
    //    params3.Add("LogoLarge", team.logolarge);
    //    db.Execute(query3, params3);
    //  }
    //}

    //[TestMethod]
    //public async Task TestInsertScheduledMatches()
    //{
    //  Console.WriteLine("np");
    //  var matches = await _fapi.GetScheduledMatches(56861);
    //  int matchescount = matches.Count();
    //  int dbcount = 0;

    //  string query1 = "INSERT or IGNORE INTO ScheduledMatches(Id, TournamentId, Position, TRound, Created, Modified, ResultId, ATeamId, BTeamId) values(@Id, @TournamentId, @Position, @TRound, @Created, @Modified, @ResultId, @ATeamId, @BTeamId)";
      
    //  using (var db = new SQLiteConnection(_config["ConnectionString"]))
    //  {
    //    Console.WriteLine("hello");
    //    foreach (var match in matches)
    //    {
    //      // Add scheduled match
    //      DynamicParameters params1 = new DynamicParameters();
    //      params1.Add("Id", match.Id);
    //      params1.Add("TournamentId", match.tournamentId);
    //      params1.Add("Position", match.position);
    //      params1.Add("TRound", match.round);
    //      params1.Add("Created", match.created);
    //      if (match.modified != null)
    //      {
    //        params1.Add("Modified", match.modified);
    //      }
    //      else
    //      {
    //        params1.Add("Modified", null);
    //      }
    //      if (match.result != null)
    //      {
    //        params1.Add("ResultId", match.result.id);
    //      }
    //      else
    //      {
    //        params1.Add("ResultId", null);
    //      }
    //      params1.Add("ATeamId", match.teams[0].id);
    //      params1.Add("BTeamId", match.teams[1].id);

    //      // Add result

    //      try
    //      {
    //      dbcount =+ db.Execute(query1, params1);

    //      }
    //      catch(Exception ex)
    //      {
    //        Console.WriteLine(ex.ToString());
    //      }
    //    }
    //  }

    //  Assert.AreEqual(matchescount, dbcount);
    //}

    //[TestMethod]
    //public void TestGetRunningGamesFromDatabase()
    //{
    //  using (var db = new SQLiteConnection(_config["ConnectionString"]))
    //  {
    //    //Fumbbl fb = new Fumbbl();
    //    //fb.GetRunningGames(db);
    //    var lookup = new Dictionary<int, RunningGame>();
    //    db.Query<RunningGame, RunningGameTournament, RunningGameTeam, RunningGame>(Properties.Resources.TestSelectAllFromRunningGame,
    //      (g, tt, t) =>
    //      {
    //        RunningGame game;
    //        if (!lookup.TryGetValue(g.RunningGame_Id, out game))
    //        {
    //          lookup.Add(g.RunningGame_Id, game = g);
    //        }
    //        if (game.teams == null)
    //        {
    //          game.teams = new List<RunningGameTeam>();
    //        }
    //        game.teams.Add(t);
    //        if (tt != null)
    //        {
    //          game.tournament = tt;
    //        }
    //        return game;
    //      }, splitOn: "RunningGame_Id,RunningGameTournament_Id,RunningGameTeam_Id").AsQueryable();
    //    var lol = lookup.Values;
    //    Assert.IsNotNull(lol);
    //    Assert.IsTrue(lol.Count() > 0);
    //    int i = 1;
    //    foreach (var meme in lol)
    //    {
    //      Assert.IsNotNull(meme.RunningGame_Id);
    //      Assert.IsNotNull(meme.teams);

    //      foreach (var team in meme.teams)
    //      {
    //        Assert.IsNotNull(team);
    //        Assert.IsNotNull(team.RunningGameTeam_Id);
    //        Assert.IsNotNull(team.side);
    //        Assert.IsNotNull(team.name);
    //        Assert.IsNotNull(team.coach);
    //        Assert.IsNotNull(team.race);
    //        Assert.IsNotNull(team.tv);
    //        Assert.IsNotNull(team.rating);
    //        Assert.IsNotNull(team.score);
    //        Assert.IsNotNull(team.logo);
    //        Assert.IsNotNull(team.logolarge);
    //      }
          

    //      if (meme.tournament != null)
    //      {
    //        Assert.IsNotNull(meme.tournament.RunningGameTournament_Id);
    //        Assert.IsNotNull(meme.tournament.group);
    //      }

    //      Console.WriteLine($"{i} / {meme.RunningGame_Id}: [{meme.teams[0].name}] vs [{meme.teams[1].name}] \\ {meme.tournament?.RunningGameTournament_Id}");
    //      i++;
    //    }
    //  }
    //}

    [TestMethod]
    public void TestDeleteRunningGameTeamsById()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        DynamicParameters args = new DynamicParameters();
        args.Add("RunningGameId", 1497011);
        var affectedRows = db.Execute(Properties.Resources.TestDeleteRunningGameTeams, args);
        Console.WriteLine("Affected Rows: "+affectedRows);
      }
    }

    [TestMethod]
    public void TestDeleteRunningGame()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        DynamicParameters args = new DynamicParameters();
        args.Add("RunningGameId", 1497011);
        var affectedRows = db.Execute(Properties.Resources.TestDeleteRunningGame, args);
        Console.WriteLine("Affected Rows: "+affectedRows);
      }
    }
  }
}
