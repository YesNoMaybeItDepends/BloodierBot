using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class RunningGameTournament
  {
    [JsonPropertyName("id")]
    public int RunningGameTournament_Id { get; set; }
    public int group { get; set; }
  }
}
