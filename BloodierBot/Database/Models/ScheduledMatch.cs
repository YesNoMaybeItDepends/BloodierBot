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
using Discord;

namespace BloodierBot.Database.Models
{
  public class ScheduledMatch
  {
    public const string API_GET_SCHEDULED_MATCHES = "https://fumbbl.com/api/tournament/schedule/";
    private int? _Id = null;
    public int? Id
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

    public int calculateId()
    {
      if (tournamentId != null && round != null && teams[0].id != null && teams[1].id != null)
      {
        string tid = tournamentId.ToString().Substring(tournamentId.ToString().Count() - 2);
        string r = round.ToString();
        string at = teams[0].id.ToString().Substring(teams[0].id.ToString().Count() - 2);
        string bt = teams[1].id.ToString().Substring(teams[1].id.ToString().Count() - 2);
        return int.Parse(tid + r + at + bt);
      }
      else
      {
        return -1;
      }
    }

    public void manualId(int id)
    {
      _Id = id;
    }

    public class Team
    {
      public int id { get; set; }
      public string name { get; set; }

      public char AorB { get; set; }
    }

    public static async Task<List<ScheduledMatch>> GetScheduledMatchesFromTournamentId(int tournamentId)
    {
      FumbblApi fapi = new FumbblApi();

      string result = null;
      List<ScheduledMatch> scheduledMatches = new List<ScheduledMatch>();

      result = await fapi.CallFumbblApi(API_GET_SCHEDULED_MATCHES + tournamentId);

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
        scheduledMatches = JsonSerializer.Deserialize<List<ScheduledMatch>>(result);
      }

      foreach (ScheduledMatch match in scheduledMatches)
      {
        match.tournamentId = tournamentId;
        match.calculateId();
      }

      return scheduledMatches;
    }

    public static async Task<ScheduledMatch?> GetScheduledMatchFromDatabase(int id, IDbConnection db)
    {
      ScheduledMatch? scheduledMatch = null;

      DynamicParameters p = new DynamicParameters();
      p.Add("Id", id);
      dynamic dbmatch = (await db.QueryAsync<dynamic>(Properties.Resources.selectScheduledGame, p));
      if (dbmatch != null)
      {
        int _id = (int)(dbmatch[0].Id);
        int _tid = (int)(dbmatch[0].TournamentId);

        int team1id = (int)(dbmatch[0].Id);
        int team2id = (int)(dbmatch[1].Id);

        char team1char = (char)(dbmatch[0].AorB[0]);
        char team2char = (char)(dbmatch[1].AorB[0]);

        Team team1 = new Team { id = team1id, name = dbmatch[0].Name , AorB = team1char};
        Team team2 = new Team { id = team2id, name = dbmatch[1].Name, AorB = team2char};
        
        List<Team> teams = new List<Team>();
        teams.Add(team1);
        teams.Add(team2);
        
        scheduledMatch = new ScheduledMatch { _Id = _id, tournamentId = _tid, teams = teams };
      }
      return scheduledMatch;
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

    public async void dbDelete(IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", Id);

      await db.ExecuteAsync(Properties.Resources.deleteScheduledMatch, p);
    }

    public static async Task<List<Embed>> EmbedPendingGames(List<ScheduledMatch> matches)
    {
      List<Embed> embeds = new List<Embed>();
      EmbedBuilder eb = new EmbedBuilder();
      FumbblApi fapi = new FumbblApi();

      int lenght = matches.Count;
      int count = 0;
      int embedCount = 1;
      decimal maxEmbeds = Math.Round((decimal)lenght / 5);
      maxEmbeds = maxEmbeds == 0 ? 1 : maxEmbeds;
      eb.WithTitle($"Matches ({0}/{1})");
      eb.WithColor(Color.DarkPurple);
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      while (count < lenght)
      {
        Models.Team? teamA = await fapi.GetThing<Models.Team>(matches[count].teams[0].id) as Models.Team;
        Models.Team? teamB = await fapi.GetThing<Models.Team>(matches[count].teams[1].id) as Models.Team;

        if (teamA != null && teamB != null)
        {
          int? id = matches[count].Id;
          string aTeamName = teamA.name;
          string aTeamCoach = teamA.coach.name;
          string aTeamRace = teamA.roster.name;
          string bTeamName = teamB.name;
          string bTeamCoach = teamB.coach.name;
          string bTeamRace = teamB.roster.name;

          eb.AddField($":id: {id}", $"**{aTeamName}** *vs* **{bTeamName}**", false);
          eb.AddField(aTeamCoach, aTeamRace, true);
          eb.AddField(bTeamCoach, bTeamRace, true);
        }
        else
        {
          return embeds = new List<Embed>();
        }

        if (count != 0 && count % 5 == 0)
        {
          embeds.Add(eb.Build());
          eb = new EmbedBuilder();
          embedCount++;
          eb.WithTitle($"Matches ({0}/{1})");
          eb.WithColor(Color.DarkPurple);
          eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
        }
        count++;
      }
      if (eb.Fields.Count != 0)
      {
        embeds.Add(eb.Build());
        return embeds;
      }
      return embeds = new List<Embed>();
    }


    public async Task<bool> isRunningDb(IDbConnection db)
    {
      bool isRunningDb = false;

      DynamicParameters p = new DynamicParameters();
      p.Add("Team1", teams[0].id);
      p.Add("Team2", teams[1].id);

      var lol = (await db.QueryAsync<dynamic>(Properties.Resources.selectRunningGameFromScheduledTeams, p));

      if (lol.FirstOrDefault() != null)
      {
        isRunningDb = true;
      }
      return isRunningDb;
    }
  }
}
