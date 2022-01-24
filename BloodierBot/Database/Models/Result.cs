using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class Result
  {
    public int id { get; set; }
    public int replayId { get; set; }
    public string winner { get; set; }
    /// <summary>
    /// id, score
    /// </summary>
    //public Dictionary<int, int> teams { get; set; }
    public List<Team> teams { get; set; }

    public class Team
    {
      public int id { get; set; }
      public int score { get; set; }
    }
  }

  
}
