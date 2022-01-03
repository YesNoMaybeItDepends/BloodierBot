using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BloodierBot.Services;
using BloodierBot.Database.Models;
using AutoMapper.Configuration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class RunningGame : IEquatable<RunningGame>
  {
    [JsonPropertyName("id")]
    public int RunningGame_Id { get; set; }
    public int half { get; set; }
    public int turn { get; set; }
    public string division { get; set; }
    public RunningGameTournament? tournament { get; set; } = null;
    public List<RunningGameTeam> teams { get; set; }

    public bool Equals(RunningGame? other)
    {
      if (other is null)
        return false;

      return RunningGame_Id == other.RunningGame_Id;
    }
    public override bool Equals(object? obj) => Equals(obj as RunningGame);
    public override int GetHashCode() => (RunningGame_Id).GetHashCode();
  }
}
