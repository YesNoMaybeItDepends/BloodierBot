using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class Bet
  {
    public ulong UserId { get; set; }
    public int MatchId { get; set; }
    public int AteamScore { get; set; }
    public int BteamScore { get; set; }
    public int Money { get; set; }
  
    public enum Winner
    {
      A,
      B,
      T
    }

    public static async Task<bool> MakeBet(ulong userId, int matchId, int aTeamScore, int bTeamScore, int money, IDbConnection db)
    {
      bool result = false;
      DynamicParameters p = new DynamicParameters();
      p.Add("UserID", userId);
      p.Add("MatchId", matchId);
      p.Add("AteamScore", aTeamScore);
      p.Add("BteamScore", bTeamScore);
      p.Add("Money", money);

      result = await db.ExecuteAsync(Properties.Resources.insertOrUpdateBet, p) == 1 ? true : false;

      return result;
    }

    public static async Task<String> BetCommand(ulong userId, int matchId, string score, int money, IDbConnection db)
    {
      StringBuilder sb = new StringBuilder();

      string[] scores = score.Split('-');
      int AteamScore = int.Parse(scores[0]);
      int BteamScore = int.Parse(scores[1]);

      bool success = false;
      if (money > 0)
      {
          int? userMoney = await User.getMoney(userId, db);

          // If user exists and has enough money
          if (userMoney != null && (userMoney - money) >= 0)
          {
            // if match exists
            ScheduledMatch? match = await ScheduledMatch.GetScheduledMatchFromDatabase(matchId, db);
            if (match != null)
            {
              // if game is already running
              if (!await match.isRunningDb(db))
              {
                await User.updateMoney(userId, -money, db);
                success = await Bet.MakeBet(userId, matchId, AteamScore, BteamScore, money, db);
                if (success)
                {
                
                  sb.AppendLine($"bet {money}★ on **{match.teams[0].name}** vs **{match.teams[1].name}**");
                }
                else
                {
                  sb.AppendLine($"Something went wrong placing the bet.");
                }
              }
              else
              {
                sb.AppendLine($"You can't bet on a running game");
              }
            }
            else
            {
              sb.AppendLine($"Invalid match id");
            }
          }
          else
          {
            sb.AppendLine($"You don't have enough money");
          }
        }
        else
        {
          sb.AppendLine($"Invalid amount of money");
        }
      return sb.ToString();
    }

    public static async Task<List<Bet>?> GetUserBets(ulong userId, IDbConnection db)
    {
      List<Bet> bets;
      DynamicParameters p = new DynamicParameters();
      p.Add("UserId", userId);
      var result = await db.QueryAsync<Bet>(Properties.Resources.selectBets, p);
      bets = result.ToList();
      return bets;
    }

    public static async Task<Bet?> GetUserMatchBet(ulong userId, int matchId, IDbConnection db)
    {
      //Bet? bet;
      DynamicParameters p = new DynamicParameters();
      p.Add("UserId", userId);
      p.Add("MatchId", matchId);
      var bet = await db.QueryAsync<Bet>(Properties.Resources.selectBet, p);
      return bet.FirstOrDefault();
    }

    public static async Task<bool> DeleteBet(ulong userId, int matchId, IDbConnection db)
    {
      bool result = false;
      DynamicParameters p = new DynamicParameters();
      p.Add("UserId", userId);
      p.Add("MatchId", matchId);
      result = await db.ExecuteAsync(Properties.Resources.deleteBet, p) == 1? true : false;
      return result;
    }

    public static async Task<List<Bet>> GetMatchBets(int matchId, IDbConnection db, Winner? betOutcome = null)
    {
      List<Bet> bets = new List<Bet>();

      DynamicParameters p = new DynamicParameters();
      p.Add("MatchId", matchId);

      switch (betOutcome)
      {
        case null:
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsA, p)).ToList());
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsB, p)).ToList());
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsT, p)).ToList());
          break;
        case Winner.A:
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsA, p)).ToList());
          break;
        case Winner.B:
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsB, p)).ToList());
          break;
        case Winner.T:
          bets.AddRange((await db.QueryAsync<Bet>(Properties.Resources.selectBetsT, p)).ToList());
          break;
        default:
          break;
      }

      return bets;
    }
  }
}
