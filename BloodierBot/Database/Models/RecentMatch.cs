using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class RecentMatch
  {
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
  }
}
