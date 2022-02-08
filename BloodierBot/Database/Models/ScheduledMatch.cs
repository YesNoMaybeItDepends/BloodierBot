using BloodierBot.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BloodierBot.Database.Models;

namespace BloodierBot.Database.Models
{
  public class ScheduledMatch
  {
    public const string API_GET_SCHEDULED_MATCHES = "https://fumbbl.com/api/tournament/schedule/";
    private long? _Id = null;
    public long? Id 
    { 
      get
      {
        if (_Id != null)
        {
          return _Id;
        }
        else if (tournamentId != null && round != null && teams[0].id != null && teams[1].id != null)
        {
          _Id = calculateId();
        }
        return _Id;
      }
    }
    public int tournamentId { get; set; }
    public int position { get; set; }
    public int round { get; set; }
    public string created { get; set; }
    public string? modified { get; set; } = null;

    //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //[JsonPropertyName("result")]
    public Result? result { get; set; } = null;
    /// <summary>
    /// id, name
    /// </summary>
    //public Dictionary<int,string> teams { get; set; }
    public List<Team> teams { get; set; }

    public long calculateId()
    {
      if (tournamentId != null && round != null && teams[0].id != null && teams[1].id != null)
      {
        string tid = tournamentId.ToString().Substring(tournamentId.ToString().Count() - 3);
        string r = round.ToString();
        string at = teams[0].id.ToString().Substring(teams[0].id.ToString().Count() - 3);
        string bt = teams[1].id.ToString().Substring(teams[1].id.ToString().Count() - 3);
        return long.Parse(tid+r+at+bt);
      }
      else
      {
        return -1;
      }
    }

    public class Team
    {
      public int id { get; set; }
      public string name { get; set; }

      public char AorB { get; set; }
    }

    public static async Task<List<ScheduledMatch>> GetScheduledMatches(int id)
    {
      FumbblApi fapi = new FumbblApi();

      string result = null;
      List<ScheduledMatch> scheduledMatches = new List<ScheduledMatch>();

      result = await fapi.CallFumbblApi(API_GET_SCHEDULED_MATCHES + id);

      if (result == null)
      {
        return scheduledMatches;
      }
      else if (fapi.IsError(result))
      {
        return scheduledMatches;
      }
      else
      {
        scheduledMatches = System.Text.Json.JsonSerializer.Deserialize<List<ScheduledMatch>>(result);
      }

      foreach (ScheduledMatch match in scheduledMatches)
      {
        match.tournamentId = id;
        match.calculateId();
      }

      return scheduledMatches;
    }

    public async void dbInsert(IDbConnection db)
    {
      // Insert Scheduled Match
      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("Id", Id);
      parameters.Add("TournamentId", tournamentId);
      await db.ExecuteAsync(Properties.Resources.insertScheduledMatch, parameters);

      // Insert the teams inside Teams
      dbInsertTeams(db);

      // Insert the teams inside Scheduled_Match_Teams
      dbInsertScheduledMatchTeams(db);
    }

    private async void dbInsertTeams(IDbConnection db)
    {
      foreach (Team team in teams)
      {
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("TeamId", team.id);
        parameters.Add("Name", team.name);

        await db.ExecuteAsync(Properties.Resources.insertTeam, parameters);
      }
    }

    private async void dbInsertScheduledMatchTeams(IDbConnection db)
    {
      foreach (Team team in teams)
      {
        if (team.id == teams[0].id)
        {
          team.AorB = 'A';
        }
        else
        {
          team.AorB = 'B';
        }

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("ScheduledMatchId", Id);
        parameters.Add("TeamId", team.id);
        parameters.Add("AorB", team.AorB);

        await db.ExecuteAsync(Properties.Resources.insertScheduledMatchTeams, parameters);
      }
    }
  }
}
