using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  // TODO end my mysery replace with tournament
  public class RunningGameTournament : IEquatable<Tournament>
  {
    [JsonPropertyName("id")]
    public int RunningGameTournament_Id { get; set; }
    public int group { get; set; }

    public bool Equals(Tournament? t)
    {
      return (RunningGameTournament_Id == t.Tournament_Id);
    }
  }
}
