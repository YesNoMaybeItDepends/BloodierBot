using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class RunningGameTeam
  {
    [JsonPropertyName("id")]
    public int RunningGameTeam_Id { get; set; }
    public string side { get; set; }
    public string name { get; set; }
    public string coach { get; set; }
    public string race { get; set; }
    public int tv { get; set; }
    public string rating { get; set; }
    public int score  { get; set; }
    public string logo { get; set; }
    public string logolarge { get; set; }
  }
}
