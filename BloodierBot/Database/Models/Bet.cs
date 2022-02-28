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
  
    public static async Task<List<Bet>?> GetBets(ulong userId, IDbConnection db)
    {
      List<Bet> bets;
      DynamicParameters p = new DynamicParameters();
      p.Add("UserId", userId);
      var result = await db.QueryAsync<Bet>(Properties.Resources.selectBets, p);
      bets = result.ToList();
      return bets;
    }

    public static async Task<Bet?> GetBet(ulong userId, int matchId, IDbConnection db)
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
  }
}
