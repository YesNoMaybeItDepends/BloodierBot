using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class Team : IGetWithId
  {
    public string ApiGetByIdLink { get; set; } = "https://fumbbl.com/api/team/get/";

    public int id { get; set; }
    public IdNamePair coach { get; set; }
    public IdNamePair roster { get; set; }
    public string name { get; set; }
    public int divisionId { get; set; }
    public string division { get; set; }
    public int league { get; set; }
    public int rerolls { get; set; }
    public int ruleset { get; set; }
    public string status { get; set; }
    public int teamValue { get; set; }
    public int treasury { get; set; }
    public int fanFactor { get; set; }
    public int assistantCoaches { get; set; }
    public int cheerleaders { get; set; }
    public string apothecary { get; set; }
    public Record record { get; set; }
    //public SpecialRules specialRules { get; set; }
    public List<Player> players { get; set; }


    public class SpecialRules
    {
      // ?????
    }
    public class Record
    {
      public int games { get; set; }
      public int wins { get; set; }
      public int ties { get; set; }
      public int losses { get; set; }
    }
    
    public class IdNamePair
    {
      public int id { get; set; }
      public string name { get; set; }
    }
  }
}
