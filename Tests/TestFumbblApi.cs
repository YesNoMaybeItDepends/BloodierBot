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
    public async Task TestFumbblApiGetRunningGames()
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
          Assert.IsNotNull(meme.tournament.RunningGameTournament_Id);
          Assert.IsNotNull(meme.tournament.group);
        }

        Console.WriteLine($"{i} / {meme.RunningGame_Id}: TV {meme.teams[0].tv} [{meme.teams[0].name}] vs [{meme.teams[1].name}] TV {meme.teams[1].tv}\\ {meme.tournament?.RunningGameTournament_Id}");
        i++;
      }
    }

    [TestMethod]
    public async Task np()
    {
      int tournamentid = 55956;

      var fumbblapi = new FumbblApi();

      List<ScheduledMatch> scheduledMatches = new List<ScheduledMatch>();

      scheduledMatches = await fumbblapi.GetScheduledMatches(tournamentid);

      if (scheduledMatches.Count != 0)
      {
        Console.WriteLine("Scheduled matches: " + scheduledMatches.Count);

        foreach (var scheduledMatch in scheduledMatches)
        {
          Console.WriteLine("----");
          Console.WriteLine("Position: " + scheduledMatch.position);
          Console.WriteLine("Round: " + scheduledMatch.round);
          Console.WriteLine("Created: " + scheduledMatch.created);
          if (scheduledMatch.modified != null) { Console.WriteLine("Modified: " + scheduledMatch.modified); }
          Console.WriteLine("Team 1: " + scheduledMatch.teams.ElementAt(0).name);
          Console.WriteLine("Team 2: " + scheduledMatch.teams.ElementAt(1).name);

          if (scheduledMatch.result != null)
          {
            Console.WriteLine("Winner: " + scheduledMatch.result.winner);
            Console.WriteLine("Score: " + scheduledMatch.result.teams[0].score + "-" + scheduledMatch.result.teams[1].score);
          }
        }
      }
      else
      {
        Assert.Fail();
      }
    }
  }
}
