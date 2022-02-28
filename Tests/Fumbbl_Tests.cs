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
      var lol = await _fapi.GetThing<RecentMatch>(4362560) as RecentMatch;
      _db.Open();
      Fumbbl.ResolveBets(lol, _db);
      _db.Close();
    }
  }
}