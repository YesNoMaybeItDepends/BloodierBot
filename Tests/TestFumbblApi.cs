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
using System.ComponentModel;
using System.Text.Json;

namespace Tests
{
  [TestClass]
  public class TestFumbblApi
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

    [TestMethod]
    public async Task TestGetTeamMatches()
    {
      var games = await _fapi.GetTeamMatches(1040568);

      foreach (var game in games)
      {
        Console.WriteLine($"{game.RecentMatch_Id}: [{game.team1.name}] vs [{game.team2.name}] \\ {game.tournamentId?.ToString()}");
      }
    }

    [TestMethod]
    public async Task TestGetRecentMatch()
    {
      var lol = await _fapi.GetRecentMatch(4355268);
      foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(lol))
      {
        string name = descriptor.Name;
        object value = descriptor.GetValue(lol);
        Console.WriteLine("{0}={1}", name, value);
      }
      foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(lol.team1))
      {
        string nname = desc.Name;
        object vvalue = desc.GetValue(lol.team1);
        Console.WriteLine("{0}={1}", nname, vvalue);
      }
      Console.WriteLine("COACH:");
      Console.WriteLine(lol.team1.coach?.name);
      foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(lol.team1.coach))
      {
        string nname = desc.Name;
        object vvalue = desc.GetValue(lol.team1.coach);
        Console.WriteLine("{0}={1}", nname, vvalue);
      }
    }

    [TestMethod]
    public async Task TestGetRunningGames()
    {
      var fumbblapi = new FumbblApi();

      var livegames = await fumbblapi.GetRunningGames();
      Assert.IsNotNull(livegames);

      int i = 1;
      foreach (var meme in livegames)
      {
        Assert.IsNotNull(meme.RunningGame_Id);
        Assert.IsNotNull(meme.teams);
        Assert.IsNotNull(meme.teams[0]);
        Assert.IsNotNull(meme.teams[1]);
        Assert.IsNotNull(meme.teams[0].RunningGameTeam_Id);
        Assert.IsNotNull(meme.teams[1].RunningGameTeam_Id);
        Assert.IsNotNull(meme.teams[0].name);
        Assert.IsNotNull(meme.teams[1].name);

        if (meme.tournament != null)
        {
          Assert.IsNotNull(meme.tournament.Tournament_Id);
          Assert.IsNotNull(meme.tournament.group);
        }

        Console.WriteLine($"{i} / {meme.RunningGame_Id}: TV {meme.teams[0].tv} [{meme.teams[0].name}] vs [{meme.teams[1].name}] TV {meme.teams[1].tv}\\ {meme.tournament?.Tournament_Id}");
        i++;
      }
    }

    // TODO finish this, need to add team meme first
    [TestMethod]
    public async Task TestInsertScheduledMatchTeam()
    {
      Console.WriteLine("np");
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        int tournamentid = 55956;
        List<ScheduledMatch> scheduledMatches = new List<ScheduledMatch>();
        scheduledMatches = await ScheduledMatch.GetScheduledMatches(tournamentid);

        // For testing purposes, use first match
        ScheduledMatch scheduledMatch;
        if (scheduledMatches.Count > 0) scheduledMatch = scheduledMatches[0]; else return;

        foreach (ScheduledMatch.Team team in scheduledMatch.teams)
        {
          // Determine if team is A or B
          char AorB;
          if (scheduledMatch.teams.IndexOf(team) == 0) AorB = 'A';
          else AorB = 'B';

          DynamicParameters parameters = new DynamicParameters();
          parameters.Add("ScheduledMatchId", scheduledMatch.Id);
          parameters.Add("TeamId", team.id);
          parameters.Add("AorB");

          try
          {
            db.Execute(Properties.Resources.InsertScheduledMatchTeam, parameters);
          }
          catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
      }
    }

    [TestMethod]
    public async Task TestGetPlayer()
    {
      Player player = await _fapi.GetThing<Player>(10817232) as Player;
      Helpers.PrintObject(player);
    }

    [TestMethod]
    public async Task TestGetTeam()
    {
      Team team = await _fapi.GetThing<Team>(1061542) as Team;
      Helpers.PrintObject(team);
    }

    [TestMethod]
    public async Task TestGetTournament()
    {
      Tournament tournament = await _fapi.GetThing<Tournament>(56971) as Tournament;
      Helpers.PrintObject(tournament);
    }

    [TestMethod]
    public async Task TestGetScheduledMatches()
    {
      List<ScheduledMatch> scheduledMatches = await ScheduledMatch.GetScheduledMatches(56971);
      Helpers.PrintObject(scheduledMatches);
    }

    [TestMethod]
    public async Task TestInsertTournament()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        Tournament tournament = await _fapi.GetThing<Tournament>(56971) as Tournament;
        tournament.DbInsertTournament(db);
      }
    }

    [TestMethod]
    public async Task TestSelectAllTournaments()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        List<Tournament> ts = await Tournament.DbSelectAllTournaments(db);
        Helpers.PrintObject(ts);
      }
    }

    [TestMethod]
    public async Task TestSelectRunningGames()
    {
      using (var db = new SQLiteConnection(_config["ConnectionString"]))
      {
        var meme = RunningGame.GetRunningGamesFromDatabase(db);
        Helpers.PrintObject(meme);
      }
    }
  }
}
