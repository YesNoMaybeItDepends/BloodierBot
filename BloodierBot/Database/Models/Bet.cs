using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class Bet
  {
    public int CoachId { get; set; }
    public int MatchId { get; set; }
    public int AteamScore { get; set; }
    public int BteamScore { get; set; }
    public int Money { get; set; }
  }
}
