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
  public class RunningGame_Tests
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

    public async Task<List<RunningGame>> CreateRunningGames_default()
    {
      List<RunningGame> runningGames = await RunningGame.GetRunningGames();
      return runningGames;
    }

    [TestMethod]
    public async Task GetRunningGames_Works()
    {
      List<RunningGame> games = await RunningGame.GetRunningGames();
      foreach (var game in games)
      {
        Helpers.PrintObject(game);
      }
    }

    [TestMethod]
    public async Task InsertRunningGame_Works()
    {
      List<RunningGame> games = await CreateRunningGames_default();

      try
      {
      _db.Open();
      foreach(var game in games)
      {
        await game.DbInsertRunningGame(_db);
      }
      _db.Close();

      }catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
        _db.Close();
      }
    }

    [TestMethod]
    public async Task GetRunningGamesFromDatabase_Works()
    {
      _db.Open();
      var games = RunningGame.GetRunningGamesFromDatabase(_db);
      foreach (var game in games)
      {
        Helpers.PrintObject(game);
      }
      Assert.IsNotNull(games);
      _db.Close();
    }

    [TestMethod]
    public async Task DeleteRunningGame_Works()
    {
      _db.Open();
      var games = RunningGame.GetRunningGamesFromDatabase(_db);
      foreach (var game in games)
      {
        int deletedRows = game.DeleteRunningGame(_db);
        Assert.AreEqual(3, deletedRows);
      }
      _db.Close();
    }
  }
}
