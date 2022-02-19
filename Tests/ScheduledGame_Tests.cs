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
  public class ScheduledGame_Tests
  {
    private IConfiguration _config;
    private IDiscordClient _client;
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
    public async Task GetScheduledMatches_Works()
    {
      List<ScheduledMatch> matches = await ScheduledMatch.GetScheduledMatches(TOURNAMENT_TG);

      foreach (var match in matches)
      {
        Assert.IsNotNull(match);
        Helpers.PrintObject(match);
      }
    }

    [TestMethod]
    public async Task dbInsert_Works()
    {
      List<ScheduledMatch> matches = await ScheduledMatch.GetScheduledMatches(TOURNAMENT_TG);

      foreach (var match in matches)
      {
        _db.Open();
        match.dbInsert(_db);
        _db.Close();
      }
    }

    [TestMethod]
    public async Task dbDeleteScheduledMatch()
    {
      List<ScheduledMatch> matches = await ScheduledMatch.GetScheduledMatches(TOURNAMENT_TG);

      _db.Open();

      foreach (var match in matches)
      {
        match.dbInsert(_db);
      }

      foreach (var match in matches)
      {
        try
        {
          match.dbDelete(_db);
        }
        catch (Exception ex) { Console.WriteLine(ex); };
      }

      _db.Close();
    }
  }
}
