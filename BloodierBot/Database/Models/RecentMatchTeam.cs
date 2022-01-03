using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class RecentMatchTeam
  {
    [JsonPropertyName("id")]
    public int RecentMatchTeam_Id { get; set; }
    public string name { get; set; }
    public int score { get; set; }
    public Casualties casualties { get; set; }
    public int fanfactor { get; set; }
    public string teamValue { get; set; }
    public Coach coach { get; set; }
    public int winnings { get; set; }
    public int gate { get; set; }
    public string tournamentWeight { get; set; }
  }
}
