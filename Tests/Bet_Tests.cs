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
  public class Bet_Tests
  {
    private IConfiguration _config;
    private IDiscordClient _client;
    private FumbblApi _fapi;
    private SQLiteConnection _db;

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
    public async Task BetCommand_Valid_Works()
    {
      string result = "np";

      _db.Open();

      // Insert Tournament
      Tournament tournament = new Tournament
      {
        Tournament_Id = 56971,
        t_name = "TG Season XIX BB2020"
      };
      await tournament.DbInsertTournament(_db);
      
      // Get Recent Match
      // We need it to insert the scheduled match
      const int RECENT_MATCH_ID = 4360832;
      var recentMatch = await _fapi.GetThing<RecentMatch>(RECENT_MATCH_ID) as RecentMatch;

      // Insert Scheduled Match
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
      smatch.dbInsert(_db);

      // Add User
      User user1 = new User(1, "one");
      await user1.RegisterUser(user1, _db);

      // Add bet
      if (smatch.Id != null)
      {
        result = await Bet.BetCommand(1, (int)smatch.Id, "1-2", 100, _db);
      }


      // ????
      Console.WriteLine(result);
      Assert.AreNotEqual("np", result);

      // Clean up DB Tournament
      // Clean up DB Scheduled Match
      smatch.dbDelete(_db);
      await user1.DeleteUser(_db);


      // Clean up DB Bet
      await Bet.DeleteBet(user1.Id, (int)smatch.Id, _db);

      _db.Close();
    }
  }
}
