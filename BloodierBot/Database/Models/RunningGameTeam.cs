using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Data;
using Dapper;

namespace BloodierBot.Database.Models
{
  public class RunningGameTeam
  {
    [JsonPropertyName("id")]
    public int RunningGameTeam_Id { get; set; }
    public int RunningGameId { get; set; }
    public string side { get; set; }
    public string name { get; set; }
    public string coach { get; set; }
    public string race { get; set; }
    public int tv { get; set; }
    public string rating { get; set; }
    public int score { get; set; }
    public string logo { get; set; }
    public string logolarge { get; set; }

    public async void DbInsertRunningGameTeam(IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", RunningGameTeam_Id);
      p.Add("RunningGameId", RunningGameId);
      p.Add("Side", side);
      p.Add("Name", name);
      p.Add("Coach", coach);
      p.Add("Race", race);
      p.Add("Tv", tv);
      p.Add("Rating", rating);
      p.Add("Score", score);
      p.Add("Logo", logo);
      p.Add("LogoLarge", logolarge);
      await db.ExecuteAsync(Properties.Resources.insertRunningGameTeam, p);
    }
  }
}
