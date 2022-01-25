using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class ScheduledMatch
  {
    public long? Id { get; set; } = null;
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
    public List<Teams> teams { get; set; }

    public void calculateId()
    {
      if (tournamentId != null && round != null && teams[0].id != null && teams[1].id != 0)
      {
        string tid = tournamentId.ToString().Substring(tournamentId.ToString().Count() - 3);
        string r = round.ToString();
        string at = teams[0].id.ToString().Substring(teams[0].id.ToString().Count() - 3);
        string bt = teams[1].id.ToString().Substring(teams[1].id.ToString().Count() - 3);
        Id = long.Parse(tid+r+at+bt);
      }
      else
      {
        Id = null;
      }
    }

    public class Teams
    {
      public int id { get; set; }
      public string name { get; set; }
    }
  }
}
