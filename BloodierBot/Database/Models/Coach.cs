using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class Coach
  {
    [JsonPropertyName("id")]
    public int Coach_Id { get; set; }
    public string name { get; set; }
  }
}
