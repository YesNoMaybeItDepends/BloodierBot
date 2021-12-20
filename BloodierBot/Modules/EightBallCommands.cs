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
using BloodierBot.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BloodierBot.Modules
{
  public class EightBallCommands : ModuleBase
  {
    private readonly BloodierBotEntities _db;
    private List<String> _validColors = new List<String>();
    private readonly IConfiguration _config;
    
    public EightBallCommands(IServiceProvider services)
    {
      _db = services.GetRequiredService<BloodierBotEntities>();
      _config = services.GetRequiredService<IConfiguration>();

      _validColors.Add("green");
      _validColors.Add("red");
      _validColors.Add("blue");
    }
    
    [Command("add")]
    public async Task AddResponse(string answer, string color)
    {
      var sb = new StringBuilder();
      var embed = new EmbedBuilder();

      var user = Context.User;

      if (!_validColors.Contains(color.ToLower()))
      {
        sb.AppendLine($"**Sorry, [{user.Username}], you must specify a valid color.**");
        sb.AppendLine("Valid colors are:");
        sb.AppendLine();
        foreach (var validColor in _validColors)
        {
          sb.AppendLine($"{validColor}");
        }
        embed.Color = new Color(255, 0, 0);
      }
      else
      {
          await _db.AddAsync(new EightBallAnswer
          {
            AnswerText = answer,
            AnswerColor = color.ToLower()
          });
          await _db.SaveChangesAsync();
        
        sb.AppendLine();
        sb.AppendLine("**Added answer:**");
        sb.AppendLine(answer);
        sb.AppendLine();
        sb.AppendLine("**With color:**");
        sb.AppendLine(color);
        embed.Color = new Color(0, 255, 0);
      }

        embed.Title = "Eight Ball Answer Adition";
        embed.Description = sb.ToString();

        await ReplyAsync(null, false, embed.Build());
      }

    [Command("list")]
    public async Task ListAnswers()
    {
      var sb = new StringBuilder();
      var embed = new EmbedBuilder();

      var user = Context.User;

      var answers = await _db.EightBallAnswer.ToListAsync();
      if (answers.Count > 0)
      {
        foreach (var answer in answers)
        {
          sb.AppendLine($":small_blue_diamond: [{answer.AnswerId}] **[{answer.AnswerText}]**");
        }
      }
      else
      {
        sb.AppendLine("No answers found!");
      }

      embed.Title = "Eight Ball Answer List";
      embed.Description = sb.ToString();
      
      await ReplyAsync(null, false, embed.Build());
    }

    [Command("remove")]
    public async Task RemoveAnswer(int id)
    {
      var sb = new StringBuilder();
      var embed = new EmbedBuilder();

      var user = Context.User;

      var answers = await _db.EightBallAnswer.ToListAsync();
      var answerToRemove = answers.Where(a => a.AnswerId == id).FirstOrDefault();

      if (answerToRemove != null)
      {
        _db.Remove(answerToRemove);
        await _db.SaveChangesAsync();
        sb.AppendLine($"Removed answer -> [{answerToRemove.AnswerText}]");
      }
      else
      {
        sb.AppendLine($"Did not find answer with id [**{id}** in the database]");
        sb.AppendLine($"Perhaps use the {_config["prefix"]}list command to list out answers");
      }

      embed.Title = "Eight Ball Answer List";
      embed.Description = sb.ToString();

      await ReplyAsync(null, false, embed.Build());
    }

    // 8Ball 
    [Command("A8ball")]
    public async Task MEME([Remainder] string args = null)
    {
      var sb = new StringBuilder();
      var embed = new EmbedBuilder();

      var replies = await _db.EightBallAnswer.ToListAsync();

      embed.WithColor(Color.Magenta);
      embed.Title = "Welcome to the 8-ball!";

      sb.AppendLine($"[{Context.User},]");
      sb.AppendLine("");

      if (args == null)
      {
        sb.AppendLine("Sorry, can't answer a question you didn't ask");
      }
      else
      {
        var answer = replies[new Random().Next(replies.Count)];
        sb.AppendLine($"You asked: {args}");
        sb.AppendLine("");
        sb.AppendLine($"...your answer is {answer.AnswerText}");
      }

      embed.Description = sb.ToString();

      await ReplyAsync(null, false, embed.Build());
    }
  }
}
