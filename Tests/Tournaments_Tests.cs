﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
  public class Tournaments_Tests
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
    public async Task ApiGetById_Works()
    {
      Helpers.PrintObject(await Tournament.ApiGetById(TOURNAMENT_TG));
    }

    [TestMethod]
    public async Task DbInsertTournament_Works()
    {
      var t = await Tournament.ApiGetById(TOURNAMENT_TG);
      Console.WriteLine(t.DbInsertTournament(_db));
    }

    [TestMethod]
    public async Task DbSelectAllTournaments_Works()
    {
      Helpers.PrintObject(await Tournament.DbSelectAllTournaments(_db));
    }


  }
}