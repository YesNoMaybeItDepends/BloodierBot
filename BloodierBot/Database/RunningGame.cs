using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BloodierBot.Services;

namespace BloodierBot.Database
{
  public partial class RunningGame
  {
    [Key]
    public int id { get; set; }
    public int half { get; set; }
    public int turn { get; set; }
    public string divison { get; set; }
    // TODO what about these?
    public Tournament tournament { get; set; }
    public Team[] teams { get; set; }
  }
}
