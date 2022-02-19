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
using System.Drawing;
using BloodierBot.Services;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Net;
using BloodierBot.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BloodierBot.Modules
{
  public class ExampleCommands : ModuleBase
  {
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly FumbblApi _fapi;

    // Constructor
    public ExampleCommands(IServiceProvider services)
    {
      _config = services.GetRequiredService<IConfiguration>();
      _logger = services.GetRequiredService<ILogger<CommandHandler>>();
      _fapi = services.GetRequiredService<FumbblApi>();
      //_db = services.GetRequiredService<BloodierBotContext>();
    }

    //  [Remainder] string args = null
    //  Takes everything as 1 string
    //  !say hello world -> "hello world"
    //  vs
    // !say hello world -> "hello" "world"

    // Kill
    [Command("Kill")]
    [RequireOwner]
    public async Task Kill()
    {
      Environment.Exit(0);
    }

    [Command("add tournament")]
    [RequireOwner]
    public async Task AddTournament([Remainder] string args = null)
    {
      bool result = false;
      StringBuilder sb = new StringBuilder();

      if (int.TryParse(args, out var id))
      {
        Console.WriteLine(id);
        FumbblApi fapi = new FumbblApi();
        Tournament? tournament = await fapi.GetThing<Tournament>(id) as Tournament;
        if (tournament != null)
        {
          using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
          {
            if (await tournament.DbInsertTournament(db))
            {
              result = true;
              sb.AppendLine($"{tournament?.t_name} added");
            }
          }
        }
      }

      if (result == false)
      {
        sb.AppendLine("That didn't work");
      }

      await ReplyAsync(sb.ToString());
    }

    [Command("matches")]
    public async Task matches()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("bet")]
    public async Task bet()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("register")]
    public async Task register()
    {
      ulong id = Context.User.Id;
      string name = Context.User.Username;
      User user = new User(id,name);
      StringBuilder sb = new StringBuilder();
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        if (await user.RegisterUser(user, db))
        {
          sb.AppendLine("Succesfully registered");
        }
        else
        {
          sb.AppendLine("You already registered");
        }
      }
      await ReplyAsync(sb.ToString());
    }

    [RequireOwner]
    [Command("cheatMoney")]
    public async Task cheatMoney([Remainder] string args = null)
    {
      int money = int.Parse(args);
      StringBuilder sb = new StringBuilder();
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        if (await User.updateMoney(Context.User.Id, money, db))
        {
          sb.AppendLine("CA-CHING");
        }
        else
        {
          sb.AppendLine("uh oh hot dog");
        }
      }
      await ReplyAsync(sb.ToString());
    }
    

    [Command("faq")]
    public async Task faq()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("bets")]
    public async Task bets()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("top")]
    public async Task top()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("deletebet")]
    public async Task deletebet()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("matchodds")]
    public async Task matchodds()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("money")]
    public async Task money()
    {
      await ReplyAsync("soon goyim, soon");
    }

    [Command("polytopia")]
    public async Task polytopia()
    {
      await ReplyAsync("based");
    }

    [Command("test")]
    public async Task test()
    {
      try
      {
        if (1==1)
        {

        }
        else
        {
          throw new TimeoutException();
        }
      }
      catch (Exception ex) { Console.WriteLine(ex.ToString()); }
      await ReplyAsync("woops lol");
    }
  }
}



