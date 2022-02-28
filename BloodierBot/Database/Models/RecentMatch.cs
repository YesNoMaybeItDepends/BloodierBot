using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using BloodierBot;

namespace BloodierBot.Database.Models
{
  public class RecentMatch : IGetWithId
  {
    public string ApiGetByIdLink { get;set; } = "https://fumbbl.com/api/match/get/";

    [JsonPropertyName("id")]
    public int RecentMatch_Id { get; set; }
    public int replayId { get; set; }
    public int? tournamentId { get; set; }
    public string date { get; set; }
    public string time { get; set; }
    public int gate { get; set; }
    public string conceded { get; set; }
    public RecentMatchTeam team1 { get; set; }
    public RecentMatchTeam team2 { get; set; }
  
    public enum AorB
    {
      A,
      B
    }

    //public async Task<Dictionary<AorB, RecentMatchTeam>> mapTeamsToTournamentTeams()
    //{
    //  Dictionary<AorB, RecentMatchTeam> map = new Dictionary<AorB, RecentMatchTeam>();
    //  Tournament tournament = await Tournament.ApiGetById(tournamentId.GetValueOrDefault());
    //  tournament.
    //  return map;
    //}
  }
}
