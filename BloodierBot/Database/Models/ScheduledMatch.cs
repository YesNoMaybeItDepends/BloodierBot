using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class ScheduledMatch
  {
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

    public class Teams
    {
      public int id { get; set; }
      public string name { get; set; }
    }
  }
}
