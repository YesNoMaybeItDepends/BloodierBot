using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BloodierBot.Database.Models
{
  public class Casualties
  {
    public int bh { get; set; } = 0;
    public int si { get; set; } = 0;
    public int rip { get; set; } = 0;
  }
}
