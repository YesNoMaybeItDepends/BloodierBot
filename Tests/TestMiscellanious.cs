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
  public class TestMiscellanious
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
    public void testconfig()
    {
      foreach (KeyValuePair<string,string> keyValuePair in _config.AsEnumerable())
      {
        Console.WriteLine($"{keyValuePair.Key} : {keyValuePair.Value}");
      }
    }

    [TestCleanup]
    public void cleanup()
    {
      Console.WriteLine("Cleanup");
    }
  }
}