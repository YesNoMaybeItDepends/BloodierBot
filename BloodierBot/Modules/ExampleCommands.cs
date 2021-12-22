using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

using BloodierBot.Services;

namespace BloodierBot.Modules
{
  public class ExampleCommands : ModuleBase
  {

    // Hello
    [Command("Hello")]
    public async Task HelloCommand()
    {
      Console.WriteLine("HELLO I AM WRITTEN I AM THE WORD");
      var memes = Context.User;
      var sb = new StringBuilder();

      var user = Context.User;

      sb.AppendLine($"You are [{user}]");
      sb.AppendLine($"I must now say, World!");

      await ReplyAsync(sb.ToString());
    }

    // Kill
    [Command("Kill")]
    [RequireOwner]
    public async Task Kill()
    {
      Environment.Exit(0);
    }

    // Test
    // TODO "!test 4927943942394234" not working "argument needs to be a number"
    [Command("test")]
    public async Task test([Remainder]string args = null)
    {
      var sb = new StringBuilder();

      if (args == null)
      {
        sb.AppendLine("You must include the id number (ex: \"!test 200)\")");
      }
      // Check if arg is a number, discard result of tryparse (out _)
      else if (int.TryParse(args, out _))
      {
        var fumbbl = new Fumbbl();

        Coach coach = await fumbbl.GetCoach(int.Parse(args));
        if (coach != null)
        {
          sb.AppendLine(coach.name);
        }
        else
        {
          sb.AppendLine("Coach not found");
        }
      }
      else
      {
        sb.AppendLine("Wrong argument, argument needs to be a number");
      }

      await ReplyAsync(sb.ToString());
    }
  }
}



