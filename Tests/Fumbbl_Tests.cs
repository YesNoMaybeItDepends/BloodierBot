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
  public class Fumbbl_Tests
  {
    private IConfiguration _config;
    private IDiscordClient _client;
    private readonly IServiceProvider _services;
    private FumbblApi _fapi;
    private SQLiteConnection _db;

    private int TOURNAMENT_TG = 56971;

    [TestInitialize]
    public void setup()
    {
      Console.WriteLine("Setup");
      var _configBuilder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile(path: "config.json");
      _config = _configBuilder.Build();

      _fapi = new FumbblApi();

      _db = new SQLiteConnection(_config["ConnectionString"]);
      Console.WriteLine("Setup Finished");
      Console.WriteLine("--------");
    }

    [TestMethod]
    public async Task ResolveBets()
    {
      _db.Open();

      const int KHORNE_VS_HALFLINGS_ID = 4360832;
      var recentMatch = await _fapi.GetThing<RecentMatch>(KHORNE_VS_HALFLINGS_ID) as RecentMatch;

      Helpers.PrintObject(recentMatch);

      User user1 = new User(1, "one");
      User user2 = new User(2, "two");
      User user3 = new User(3, "three");
      int? money1 = null;
      int? money2 = null;
      int? money3 = null;

      await user1.RegisterUser(user1, _db);
      await user2.RegisterUser(user2, _db);
      await user3.RegisterUser(user3, _db);

      Tournament tournament = new Tournament
      {
        Tournament_Id = 56971,
        t_name = "TG Season XIX BB2020"
      };
      await tournament.DbInsertTournament(_db);

      ScheduledMatch smatch = new ScheduledMatch
      {
        tournamentId = 56971,
        round = 1,
        teams = new List<ScheduledMatch.Team>
        {
          new ScheduledMatch.Team
          {
            id = 1058247,
            name = "Khorne Blood Boilers",
            AorB = 'A'
          },
          new ScheduledMatch.Team
          {
            id = 1065304,
            name = "Halflings HalfPies Inc",
            AorB = 'B'
          }
        }
      };

      Helpers.PrintObject(smatch);
      smatch.dbInsert(_db);


      money1 = (await User.getMoney(1, _db));
      money2 = (await User.getMoney(2, _db));
      money3 = (await User.getMoney(3, _db));
      Console.WriteLine($"user1 money -> {money1}");
      Console.WriteLine($"user2 money -> {money2}");
      Console.WriteLine($"user3 money -> {money3}");

      await Bet.MakeBet(1, (int)smatch.Id, 1, 0, 100, _db);
      await Bet.MakeBet(2, (int)smatch.Id, 0, 1, 100, _db);
      await Bet.MakeBet(3, (int)smatch.Id, 0, 0, 100, _db);
      await User.updateMoney(1, -100, _db);
      await User.updateMoney(2, -100, _db);
      await User.updateMoney(3, -100, _db);

      money1 = (await User.getMoney(1, _db));
      money2 = (await User.getMoney(2, _db));
      money3 = (await User.getMoney(3, _db));
      Console.WriteLine($"user1 money -> {money1}");
      Console.WriteLine($"user2 money -> {money2}");
      Console.WriteLine($"user3 money -> {money3}");

      Console.WriteLine("RESOLVING BETS");
      Console.WriteLine(await Fumbbl.ResolveBets(recentMatch, _db));
      Console.WriteLine("BETS RESOLVED");

      money1 = (await User.getMoney(1, _db));
      money2 = (await User.getMoney(2, _db));
      money3 = (await User.getMoney(3, _db));
      Console.WriteLine($"user1 money -> {money1}");
      Console.WriteLine($"user2 money -> {money2}");
      Console.WriteLine($"user3 money -> {money3}");

      await user1.DeleteUser(_db);
      await user2.DeleteUser(_db);
      await user3.DeleteUser(_db);

      _db.Close();
    }

    [TestMethod]
    public async Task atest()
    {
      const int KHORNE_VS_HALFLINGS_ID = 4360832;
      var recentMatch = await _fapi.GetThing<RecentMatch>(KHORNE_VS_HALFLINGS_ID) as RecentMatch;

      Helpers.PrintObject(recentMatch);

      Tournament tournament = new Tournament
      {
        Tournament_Id = 56971,
        t_name = "TG Season XIX BB2020"
      };

      ScheduledMatch smatch = new ScheduledMatch
      {
        tournamentId = 56971,
        round = 1,
        teams = new List<ScheduledMatch.Team>
        {
          new ScheduledMatch.Team
          {
            id = 1058247,
            name = "Khorne Blood Boilers",
            AorB = 'A'
          },
          new ScheduledMatch.Team
          {
            id = 1065304,
            name = "Halflings HalfPies Inc",
            AorB = 'B'
          }
        }
      };

      var tournamentGames = await ScheduledMatch.GetScheduledMatchesFromTournamentId(recentMatch.tournamentId.GetValueOrDefault());
      Console.WriteLine("------------------------------------");
      //Helpers.PrintObject(tournamentGames);

      // Find the scheduled match 
      ScheduledMatch? scheduledMatch = tournamentGames.Find(tourneygame => tourneygame.result?.id == recentMatch.RecentMatch_Id);

      // Determine winner (A, B, or T)
      int? aTeamScore = scheduledMatch.result?.teams[0].score;
      int? bTeamScore = scheduledMatch.result?.teams[1].score;
      char winner =
        aTeamScore == bTeamScore ? 'T' :
        aTeamScore > bTeamScore ? 'A' : 'B';
      switch (winner)
      {
        case 'B':
          Console.WriteLine("np2);");
          break;
        default:
          break;
      }
    }

    public int calculatePayout(int bet, int winnerPot, int loserPot, bool isBonus)
    {
      decimal percentPot = (int)Math.Round(((double)bet / (double)winnerPot) * 100);
      decimal moneyWon = (int)Math.Round(((double)percentPot * 0.01) * (double)loserPot);
      if (isBonus)
      {
        return (int)(moneyWon + bet * 2);
      }
      else
      {
        return (int)(moneyWon + bet);
      }
    }

    [TestMethod]
    public void CalculatePayout_0_is0()
    {
      Assert.AreEqual(0, calculatePayout(0, 0, 0, false));
      Assert.AreEqual(0, calculatePayout(0, 0, 0, true));
    }

    [TestMethod]
    public void CalculatePayout_idk()
    {
      int bet = 76;
      int winnerPot = 354;
      int loserPot = 146;
      bool isBonus = false;
      int totalPayout = calculatePayout(bet,winnerPot,loserPot,isBonus);
      Console.WriteLine($"Bet: {bet}\nWon: {totalPayout - bet}\nTotal Won: {totalPayout}");
    }
  }
}