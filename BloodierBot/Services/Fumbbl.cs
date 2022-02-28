using BloodierBot.Database;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.SQLite;
using Dapper;
using BloodierBot.Database.Models;
using Discord;

namespace BloodierBot.Services
{
  public class Fumbbl
  {
    private readonly IConfiguration _config;
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly FumbblApi _fumbbl;
    //private readonly BloodierBotContext _db;

    // Constructor
    public Fumbbl(IServiceProvider services)
    {
      _config = services.GetRequiredService<IConfiguration>();
      _commands = services.GetRequiredService<CommandService>();
      _client = services.GetRequiredService<DiscordSocketClient>();
      _logger = services.GetRequiredService<ILogger<CommandHandler>>();
      _fumbbl = services.GetRequiredService<FumbblApi>();
      //_db = services.GetRequiredService<BloodierBotContext>();
      _services = services;
    }

    // Initialization
    // TODO Should be on client ready async
    public async Task InitializeAsync()
    {
      // TODO what does this do?
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    // TODO test this
    /// <summary>
    /// Inserts a ScheduledMatch + TODO also insert winner
    /// <para />
    /// Does NOT dispose Database Connection
    /// </summary>
    /// <param name="match"></param>
    /// <param name="db"></param>
    public void InsertScheduledMatch(ScheduledMatch match, IDbConnection db)
    {
      DynamicParameters params1 = new DynamicParameters();
      
      // Add basic parameters
      params1.Add("Id", match.Id);
      params1.Add("TournamentId", match.tournamentId);
      params1.Add("Position", match.position);
      params1.Add("TRound", match.round);
      params1.Add("Created", match.created);
      
      // Add modified parameter    
      if (match.modified != null)
      {
        params1.Add("Modified", match.modified);
      }
      else
      {
        params1.Add("Modified", null);
      }
      
      // Add Result parameter
      if (match.result != null)
      {
        params1.Add("ResultId", match.result.id);
      }
      else
      {
        params1.Add("ResultId", null);
      }
      
      // Add Team parameters
      params1.Add("ATeamId", match.teams[0].id);
      params1.Add("BTeamId", match.teams[1].id);

      // Add result
      db.Execute(Properties.Resources.insertScheduledMatch, params1);
    }

    public async Task run()
    {
      while (true)
      {
        StringBuilder sb = new StringBuilder();
        string newRunCycleString = "["+DateTime.UtcNow.AddHours(1).ToLongTimeString() + " DBG] : New run() cycle";
        sb.AppendLine(newRunCycleString);
        Console.WriteLine(newRunCycleString);
        // Get CurrentGames from the DB
        // Compare
        // Find games to announce
        // Find games to resolve
        try
        {
          using (IDbConnection db = new SQLiteConnection(_config["ConnectionString"]))
          {
            var livegames = await _fumbbl.GetRunningGames();
            var dbgames = RunningGame.GetRunningGamesFromDatabase(db);

            var gamesToAnnounce = livegames.Except(dbgames).ToList();
            var gamesToResolve = dbgames.Except(livegames).ToList();

            // Resolving games
            foreach (var game in gamesToResolve)
            {
              sb.AppendLine($"{game.teams[0]?.name} vs {game.teams[1]?.name} / {game.tournament?.group}");
              // If we fail to resolve the game, try again in the next run
              if (await ResolveGame(game) == true)
              {
                game.DeleteRunningGame(db);
              }
            }
            
            // New games
            List<Tournament> trackedTournaments = await Tournament.DbSelectAllTournaments(db);
            var trackedTournamentIds = trackedTournaments.Select(x => x.Tournament_Id).ToList();
            
            foreach (var game in gamesToAnnounce)
            {
              if (game.tournament != null && trackedTournamentIds.Contains(game.tournament.Tournament_Id))
              {
                sb.AppendLine($"{game.teams[0].name} vs {game.teams[1].name} / {game.tournament?.group}");
                await game.DbInsertRunningGame(db);
                await AnnounceGame(game);
              }
            }
          }
          ulong debugChannelId = ulong.Parse(_config["Channel_Debug"]);
          var debugChannel = _client.GetChannel(debugChannelId) as SocketTextChannel;
          await debugChannel.SendMessageAsync(sb.ToString());
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("["+DateTime.UtcNow.AddHours(1).ToLongTimeString()+" DBG] : Sleeping...");
        Thread.Sleep(60000);
      }
    }

    public async Task AnnounceGame(RunningGame game)
    {
      var eb = new EmbedBuilder();

      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      //eb.WithColor(Discord.Color.DarkPurple);
      eb.WithColor(Color.Blue);
      eb.WithTitle("Spectate Game");
      eb.WithUrl($"https://fumbbl.com/ffblive.jnlp?spectate={game.RunningGame_Id}");

      // Fields
      var header = new EmbedFieldBuilder();
      header.WithName(":eye::eye:");
      header.WithValue($"**{game.teams[0].name}** vs **{game.teams[1].name}**");
      var team1 = new EmbedFieldBuilder();
      team1.WithIsInline(true);
      team1.Name = $"{game.teams[0].coach}";
      team1.Value = $"{game.teams[0].race}\n{game.teams[0].tv}";
      var team2 = new EmbedFieldBuilder();
      team2.WithIsInline(true);
      team2.Name = $"{game.teams[1].coach}";
      team2.Value = $"{game.teams[1].race}\n{game.teams[1].tv}";

      eb.AddField(header);
      eb.AddField(team1);
      eb.AddField(team2);

      ulong channelId = ulong.Parse(_config["Channel_Announce"]);
      var channel = _client.GetChannel(channelId) as SocketTextChannel;


      await channel.SendMessageAsync(MentionUtils.MentionRole(326485871639658496),embed: eb.Build());

    }

    public async Task<bool> ResolveGame(RunningGame livegame)
    {
      ulong resolvingId = ulong.Parse(_config["Channel_Resolve"]);
      ulong errorsId = ulong.Parse(_config["Channel_Error"]);
      var channelResolving = _client.GetChannel(resolvingId) as SocketTextChannel;
      var channelErrors = _client.GetChannel(errorsId) as SocketTextChannel;
      var game = await FindRecentMatchFromRunningGame(livegame);
      
      if (game != null)
      {
        await channelResolving.SendMessageAsync(embed: EmbedRecentMatch(game));
        return true;
      }
      else
      {
        // retry
        await channelErrors.SendMessageAsync("@ItDepends WARNING ERROR OH GOD"+livegame.teams[0]+"/"+ livegame.teams[1]);
        return false;
      }
    }

    public static async void ResolveBets(RecentMatch match, IDbConnection db)
    {
      // Get all games in the same tournament
      var tournamentGames = await ScheduledMatch.GetScheduledMatchesFromTournamentId(match.tournamentId.GetValueOrDefault());
      
      // Find the scheduled match 
      ScheduledMatch? scheduledMatch = tournamentGames.Find(tourneygame => tourneygame.result?.id == match.RecentMatch_Id);
      
      // Determine winner (A, B, or T)
      int? aTeamScore = scheduledMatch.result?.teams[0].score;
      int? bTeamScore = scheduledMatch.result?.teams[1].score;
      char winner = 
        aTeamScore == bTeamScore ? 'T' : 
        aTeamScore > bTeamScore ? 'A' : 'B';
      
      // Get winner and loser pots
      List<Bet> winnerBets = new List<Bet>();
      List<Bet> loserBets = new List<Bet>();
        
      DynamicParameters p = new DynamicParameters();
      p.Add("MatchId", scheduledMatch.result?.id);
      switch (winner)
      {
        case 'A':
          winnerBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsA, p)).ToList());
          loserBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsB, p)).ToList());
          loserBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsT, p)).ToList());
          break;
        case 'B':
          winnerBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsB, p)).ToList());
          loserBetshttps://github.com/YesNoMaybeItDepends/BloodBot/blob/master/SQL.cs.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsA, p)).ToList());
          loserBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsT, p)).ToList());
          break;
        case 'T':
          winnerBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsT, p)).ToList());
          loserBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsA, p)).ToList());
          loserBets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsB, p)).ToList());
          break;
        default:
          break;
      }

      int winnerPot = 0+winnerBets.Sum(bet => bet.Money);
      int loserPot = 0+loserBets.Sum(bet => bet.Money);
      int totalPot = 0+winnerPot + loserPot;

      // No one bet
      if (totalPot == 0)
      {
        Console.WriteLine("No one cared");
      }
      // Everyone lost
      else if (winnerPot == 0)
      {
        Console.WriteLine("Everyone lost");
        foreach (Bet bet in loserBets)
        {
          // Delete bet
          await Bet.DeleteBet(bet.UserId, scheduledMatch.result.id, db);

          // Broke check
          // If broke and has no bets
          if (await User.getMoney(bet.UserId,db) == 0 && (await Bet.GetBets(bet.UserId,db)).Count == 0)
          {
            Console.WriteLine($"{bet.UserId} went broke here's some money");
            await User.updateMoney(bet.UserId, 50, db);
          }
          else
          {
            Console.WriteLine("lost money");
          }
        }
      }
      // Everyone won
      else if (loserPot == 0)
      {
        foreach (Bet bet in winnerBets)
        {
          await User.updateMoney(bet.UserId, bet.Money, db);
          await Bet.DeleteBet(bet.UserId, scheduledMatch.result.id, db);
          Console.WriteLine("Everyone won so no one wins anything, you get your money back");
        }
      }
      // Winners & Losers
      else
      {
        
        // Winners
        foreach (Bet bet in winnerBets)
        {
          decimal percentPot = (int)Math.Round(((double)bet.Money / (double)winnerPot) * 100);
          decimal moneyWon = (int)Math.Round(((double)percentPot * 0.01) * (double)loserPot);
          // if the bet matches the score, double the reward
          if (bet.AteamScore == aTeamScore && bet.BteamScore == bTeamScore)
          {
            int totalMoneyWon = (int)(moneyWon + bet.Money * 2);
            await User.updateMoney(bet.UserId, totalMoneyWon, db);
            await Bet.DeleteBet(bet.UserId, bet.MatchId, db);
            Console.WriteLine("woah double money!!!! "+ totalMoneyWon);
          }
          // else normal reward
          else
          {
            int totalMoneyWon = (int)(moneyWon + bet.Money);
            await User.updateMoney(bet.UserId, totalMoneyWon, db);
            await Bet.DeleteBet(bet.UserId, bet.MatchId, db);
            Console.WriteLine("won normal money " + totalMoneyWon);
          }
        }

        // Losers
        foreach (Bet bet in loserBets)
        {
          // Delete bet
          await Bet.DeleteBet(bet.UserId, scheduledMatch.result.id, db);

          // Broke check
          // If broke and has no bets
          if (await User.getMoney(bet.UserId, db) == 0 && (await Bet.GetBets(bet.UserId, db)).Count == 0)
          {
            Console.WriteLine($"{bet.UserId} went broke here's some money");
            await User.updateMoney(bet.UserId, 50, db);
          }
          else
          {
            Console.WriteLine("lost money");
          }
        }
      }

      Console.WriteLine("Finished resolving");
      Console.WriteLine("send discord message here");
    }

    public async Task<RecentMatch> FindRecentMatchFromRunningGame(RunningGame game)
    {
      Console.WriteLine("Finding recent match from running games");

      RecentMatch recentGame = null;

      var matchHistory = await _fumbbl.GetTeamMatches(game.teams[0].RunningGameTeam_Id);
      if (matchHistory != null)
      {
        Console.WriteLine("Obtained home team match history");
      }
      else
      {
        Console.WriteLine("Failed to obtain home tema match history");
      }

      var homeTeamName = game.teams[0].name;
      var homeTeamTV = game.teams[0].tv;
      Console.WriteLine("Home team: "+homeTeamName);

      var awayTeamName = game.teams[1].name;
      var awayTeamTV = game.teams[1].tv;
      Console.WriteLine("Away team: "+awayTeamName);

      if (matchHistory != null)
      {
        recentGame = matchHistory.Find(historyGame =>
      ((historyGame.team1.name == homeTeamName) || (historyGame.team1.name == awayTeamName))
      &&
      ((historyGame.team2.name == homeTeamName) || (historyGame.team2.name == awayTeamName)));
      }
      if (recentGame != null)
      {
        return recentGame;
      }
      else
      {
        Console.WriteLine("Could not find recentgame in home team's match history");
        ulong errorsId = ulong.Parse(_config["Channel_Error"]);
        var channelErrors = _client.GetChannel(errorsId) as SocketTextChannel;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Could not find recentgame in home team's match history");
        sb.AppendLine($"{homeTeamName} vs {awayTeamName}");
        sb.AppendLine("Half: " + game.half);
        sb.AppendLine("Turn: " + game.turn);
        await channelErrors.SendMessageAsync(sb.ToString());
        return recentGame;
      }
    }

    public Embed EmbedRecentMatch(RecentMatch game)
    {
      var eb = new EmbedBuilder();
      var team1 = new EmbedFieldBuilder();
      var vs = new EmbedFieldBuilder();
      var team2 = new EmbedFieldBuilder();
      var coach1 = new EmbedFieldBuilder();
      var misc = new EmbedFieldBuilder();
      var coach2 = new EmbedFieldBuilder();
      var footer = new EmbedFieldBuilder();

      // Team 1
      team1.WithIsInline(true);
      team1.WithName($"{game.team1.name}");
      team1.WithValue("\u200b");

      // Vs
      vs.WithIsInline(true);
      vs.WithName($"{game.team1.score} - {game.team2.score}");
      vs.WithValue("\u200b");

      // Team 2
      team2.WithIsInline(true);
      team2.WithName($"{game.team2.name}");
      team2.WithValue("\u200b");

      // Coach 1
      coach1.WithIsInline(true);
      coach1.WithName($"{game.team1.coach.name}");
      var coach1string = new StringBuilder();
      coach1string.AppendLine("*Race*");
      coach1string.AppendLine(game.team1.teamValue);
      coach1.WithValue($"{coach1string}");

      // Misc
      misc.WithIsInline(true);
      misc.WithName("\u200b");
      misc.WithValue("\u200b");

      // Coach 2
      coach2.WithIsInline(true);
      coach2.WithName($"{game.team2.coach.name}");
      var coach2string = new StringBuilder();
      coach2string.AppendLine("*Race*");
      coach2string.AppendLine(game.team2.teamValue);
      coach2.WithValue($"{coach2string}");

      // Footer
      footer.WithIsInline(false);
      footer.WithName("\u200b");
      footer.WithValue($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");

      // Embed
      eb.WithThumbnailUrl("https://i.imgur.com/QTzpQlD.png");
      eb.WithColor(Color.Green);
      eb.WithTitle("Match Result");
      eb.WithUrl($"https://fumbbl.com/p/match?id={game.RecentMatch_Id}");
      eb.WithDescription($"\n");
      //eb.WithDescription($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");
      eb.AddField(team1);
      eb.AddField(vs);
      eb.AddField(team2);
      eb.AddField(coach1);
      eb.AddField(misc);
      eb.AddField(coach2);
      eb.AddField(footer);
      //eb.WithFooter($"[Replay](https://fumbbl.com/ffblive.jnlp?replay={game.replayId})");

      return eb.Build();
    }
  }
}
