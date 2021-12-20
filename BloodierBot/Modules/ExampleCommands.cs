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
      System.Environment.Exit(0);
    }
  }
}



