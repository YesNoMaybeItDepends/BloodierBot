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
        var fumbbl = new FumbblApi();

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

    [Command("Live")]
    [RequireOwner]
    public async Task Live()
    {
      var sb = new StringBuilder();
      FumbblApi fumbbl = new FumbblApi();

      var games = await fumbbl.GetRunningGames();

      if (games != null)
      {
        foreach (var game in games)
        {
          sb.AppendLine($"{game.teams[0].name} vs {game.teams[1].name}");
          sb.AppendLine("https://fumbbl.com/ffblive.jnlp?spectate="+game.RunningGame_Id);
          sb.AppendLine();
        }
      }
      else
      {
        sb.AppendLine("No live games found");
      }

      await ReplyAsync(sb.ToString());
    }

    [Command("load coaches")]
    [RequireOwner]
    public async Task LoadCoaches()
    {
      StringBuilder sb = new StringBuilder();

      string connectionString = @"Data Source=.\BloodierBot.db";
      using (IDbConnection db = new SQLiteConnection(connectionString))
      {
        //db.Open();
        //var command = new SQLiteCommand("Select * from Coach");
        //command.Execute
        var output = db.Query<Coach>("Select * from Coach").ToList();
        foreach(var meme in output)
        {
          sb.AppendLine(meme.name);
        }
      }

      await ReplyAsync(sb.ToString());
    }

    [Command("save coach")]
    [RequireOwner]
    public async Task SaveOwner([Remainder] string args = null)
    {
      Console.WriteLine(args);
      args = "meme";
      string connectionString = @"Data Source=.\BloodierBot.db";
      using (IDbConnection db = new SQLiteConnection(connectionString))
      {
        //db.Open();
        //SQLiteCommand command = new SQLiteCommand("Insert into Coach (Name) values (@Name)", db);
        //command.Parameters.AddWithValue("@Name", args);
        //command.ExecuteNonQuery();
        //db.Close();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("Name", "lol", DbType.String, ParameterDirection.Input);
        db.Execute("Insert into Coach (Name) values (@Name)", parameters);
      }

      return;
    }

    [Command("testma")]
    [RequireOwner]
    public async Task Testma()
    {
      var eb = new EmbedBuilder();
      
      // Fields
      var header = new EmbedFieldBuilder();
      header.WithName(":eye::eye:");
      header.WithValue("**Frosted Furry Fillers** vs **First World Chaos**");
      var team1 = new EmbedFieldBuilder();
      team1.WithIsInline(true);
      team1.Name = "antg";
      team1.Value = "Skaven \n 1400";
      var team2 = new EmbedFieldBuilder();
      team2.WithIsInline(true);
      team2.Name = "Domunperg";
      team2.Value = "Chaos Chosen \n 1400";
      
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.WithColor(Discord.Color.DarkPurple);
      eb.WithTitle("Spectate Game");
      eb.WithUrl("https://i.imgur.com/QTzpQlD.png");
      //eb.WithDescription("lmao");
      
      eb.AddField(header);
      eb.AddField(team1);
      eb.AddField(team2);

      //eb.WithImageUrl("https://i.imgur.com/Fa2VtT6.jpg");
      var e = eb.Build();

      await ReplyAsync("@Lazyfan", embed: e);
    }
  }
}



