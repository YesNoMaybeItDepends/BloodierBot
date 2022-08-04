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
  public class Commands : ModuleBase
  {
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly FumbblApi _fapi;

    // Constructor
    public Commands(IServiceProvider services)
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

    [Command("addtournament")]
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
      int tournamentId = int.Parse(_config["TournamentId"]);
      List<ScheduledMatch> allMatches = await ScheduledMatch.GetScheduledMatchesFromTournamentId(tournamentId);
      List<ScheduledMatch> pendingMatches = allMatches.Where(match => match.result == null).ToList();
      List<Embed> embeds = await ScheduledMatch.EmbedPendingGames(pendingMatches);
      
      if (embeds.Count > 0)
      {
        foreach(Embed embed in embeds)
        {
          await ReplyAsync(embed: embed);
        }
      }
      else
      {
        await ReplyAsync("No matches found");
      }
    }

    [Command("bet")]
    public async Task bet(int matchId, string score, int money)
    {
      string result = "Couldn't establish connection to the database";
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        db.Open();
        result = await Bet.BetCommand(Context.User.Id, matchId, score, money, db);
        db.Close();
      }
      await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} {result}");
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
          sb.AppendLine(MentionUtils.MentionUser(id)+" You have been succesfully registered");
        }
        else
        {
          sb.AppendLine(MentionUtils.MentionUser(id) + " You are already registered");
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
          sb.AppendLine("KA-CHING");
        }
        else
        {
          sb.AppendLine("uh oh hot dog");
        }
      }
      await ReplyAsync(sb.ToString());
    }
    

    [Command("help")]
    public async Task help()
    {
      EmbedBuilder eb = new EmbedBuilder();
      eb.WithTitle("Help");
      eb.WithColor(Discord.Color.DarkPurple);
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.AddField(":exclamation: faq", "Explains how the whole thing works and FAQs");
      eb.AddField(":exclamation: help", "Shows this message");
      eb.AddField(":exclamation: register", "Register yourself to start betting");
      eb.AddField(":exclamation: matches", "Shows a list with all the scheduled matches as A-B and their ID");
      eb.AddField(":exclamation: money", "Shows your money");
      eb.AddField(":exclamation: bet <Match ID> <Score> <Money>", "Make a bet with the score on a match\nFor more information on how winners and losers are determined check !faq\n **#-#**: A-B\n**Example**: !bet 1 2-1 100");
      eb.AddField(":exclamation: bets", "Shows a list with all your bets");
      eb.AddField(":exclamation: deletebet <Match ID>", "Deletes a bet on the specified match and refunds your money");
      Embed e = eb.Build();
      await ReplyAsync(string.Empty, embed: e);
    }

    [Command("faq")]
    public async Task faq()
    {
      EmbedBuilder eb = new EmbedBuilder();
      eb.WithTitle("FAQ");
      eb.WithColor(Discord.Color.DarkPurple);
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.AddField("How do I bet?", Properties.Resources.HowIsBetFormed);
      eb.AddField("How are winners determined, and how does Outcome/Score matter?", Properties.Resources.HowIsWinnerBorn);
      eb.AddField("How are payouts calculated?", Properties.Resources.HowIsPayoutCalculated);
      Embed e = eb.Build();
      await ReplyAsync(string.Empty, embed: e);
    }

    [Command("bets")]
    public async Task bets()
    {
      StringBuilder sb = new StringBuilder();
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        List<Bet> bets = await Bet.GetUserBets(Context.User.Id, db);
        foreach (Bet bet in bets)
        {
          sb.AppendLine($"{bet.UserId} -> {bet.MatchId}: {bet.AteamScore}-{bet.BteamScore} for {bet.Money}");
        }
      }
      await ReplyAsync(sb.ToString());
    }

    [Command("top")]
    public async Task top()
    {
      await ReplyAsync("soon");
    }

    [Command("deletebet")]
    public async Task deletebet(int matchId)
    {
      StringBuilder sb = new StringBuilder();
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        Bet? bet = await Bet.GetUserMatchBet(Context.User.Id, matchId, db);
        if (bet != null)
        {
          if (await Bet.DeleteBet(Context.User.Id, matchId, db))
          {
            await User.updateMoney(Context.User.Id, bet.Money, db);
            sb.AppendLine("Bet succesfully deleted");
          }
        }
        else
        {
          sb.AppendLine("No bet with that Match ID");
        }
      }
      await ReplyAsync(sb.ToString());
    }

    [Command("matchodds")]
    public async Task matchodds(int matchid)
    {
      StringBuilder sb = new StringBuilder();

      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        var match = await ScheduledMatch.GetScheduledMatchFromDatabase(matchid, db);
        if (match != null)
        {
          List<Bet> betsA = await Bet.GetMatchBets(matchid, db, Bet.Winner.A);
          List<Bet> betsB = await Bet.GetMatchBets(matchid, db, Bet.Winner.B);
          List<Bet> betsT = await Bet.GetMatchBets(matchid, db, Bet.Winner.T);

          decimal potA = 0;
          decimal potB = 0;
          decimal potT = 0;

          foreach (Bet bet in betsA)
          {
            potA += bet.Money;
          }

          foreach (Bet bet in betsB)
          {
            potB += bet.Money;
          }

          foreach (Bet bet in betsT)
          {
            potT += bet.Money;
          }

          decimal totalPot = potA + potB + potT;

          if (totalPot == 0)
          {
            sb.AppendLine();
            sb.AppendLine($"**{match.teams[0].name}** vs **{match.teams[1].name}**");
            sb.AppendLine();
            sb.AppendLine($":regional_indicator_a: 0★");
            sb.AppendLine($":regional_indicator_t: 0★");
            sb.AppendLine($":regional_indicator_b: 0★");
            sb.AppendLine();
          }
        }
      }
      await ReplyAsync("soon");
    }

    [Command("money")]
    public async Task money()
    {
      StringBuilder sb = new StringBuilder();
      using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
      {
        int? money = null;
        money = await User.getMoney(Context.User.Id, db);
        if (money != null)
        {
          sb.AppendLine(money.ToString());
        }
        else
        {
          sb.AppendLine("You don't have any money, did you register?");
        }
      }
      await ReplyAsync(sb.ToString());
    }

    [Command("mresolve")]
    [RequireOwner]
    public async Task mresolve(int matchid)
    {
      StringBuilder sb = new StringBuilder();

      RecentMatch? match = await _fapi.GetThing<RecentMatch>(matchid) as RecentMatch;
      if (match is null)
      {
        sb.AppendLine("couldn't find match");
      }
      else
      {
        using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
        {
          string resolve = await Fumbbl.ResolveBets(match!, db);
          ulong righouseId = ulong.Parse(_config["Channel_Righouse"]);
          var channelRighouse = await Context.Client.GetChannelAsync(righouseId) as SocketTextChannel;
          await channelRighouse.SendMessageAsync(resolve);
          sb.AppendLine(resolve);
        }
      }
    }

    [Command("polytopia")]
    public async Task polytopia()
    {
      await ReplyAsync("based");
    }

    [RequireOwner]
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



