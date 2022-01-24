using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BloodierBot.Database.Models;

namespace BloodierBot.Services
{
  // TODO turn downloadstring into downloadstringasync
  public class FumbblApi
  {

    public const string API_GET_MATCH_INFO = "https://fumbbl.com/api/match/get/";

    public async Task<String> CallFumbblApi(string query)
    {
      string result = null;
      Uri uri = new Uri(query);

      try
      {
        using (var webclient = new WebClient())
        {
          // Handle Download Completed Event
          webclient.DownloadStringCompleted += async (sender, e) =>
          {
            if (e.Result != null)
            {
              result = e.Result;
            }
          };

          // Download the resource
          result = await webclient.DownloadStringTaskAsync(uri);
        }
      }
      catch (WebException e)
      {
        // TODO log
        Console.WriteLine("ERROR -> " + e.Message);
      }

      return result;
    }

    public async Task<RecentMatch> GetRecentMatch(int id)
    {
      string result = null;
      RecentMatch game = null;

      result = await CallFumbblApi(API_GET_MATCH_INFO+ id);

      if (result == null)
      {
        return game;
      }
      else if (IsError(result))
      {
        return game;
      }
      else
      {
        game = JsonSerializer.Deserialize<RecentMatch>(result);
        return game;
      }
    }

    public bool IsError(string result)
    {
      if (result.StartsWith("\"Error:"))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    #region boxtrophy
    #endregion

    #region Coach Section

    public const string API_GET_COACH_INFORMATION = "https://fumbbl.com/api/coach/g3t/";

    /// <summary>
    /// Find Coach by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Coach object, or null Coach object if coach was not found</returns>
    public async Task<Coach> GetCoach(int id)
    {
      string result = null;
      Coach coach = null;

      try
      {
        using (WebClient webclient = new WebClient())
        {
          // Handle Download Completed Event
          webclient.DownloadStringCompleted += async (sender, e) =>
          {
            if (e.Result != null)
            {
              result = e.Result;
            }
          };

          // Download the resource
          result = await webclient.DownloadStringTaskAsync(new Uri(API_GET_COACH_INFORMATION + id.ToString()));
        }
      }
      // 
      catch (WebException e)
      {
        Console.WriteLine("ERROR -> "+e.Message);
      }

      if (result != null)
      {
        if (!result.StartsWith("\"Error:"))
        {
          Console.WriteLine(result);
          coach = JsonSerializer.Deserialize<Coach>(result);
        }
        else
        {
          Console.WriteLine(result);
        }
      }
      return coach;
    }
    #endregion

    #region Group Section
    #endregion

    #region match

    public const string API_GET_LIVE_GAMES = "https://fumbbl.com/api/match/current";
    
    public async Task<List<RunningGame>> GetRunningGames()
    {
      string result = null;
      List<RunningGame> runningGames = new List<RunningGame>();

      result = await CallFumbblApi(API_GET_LIVE_GAMES);
    
      if (result == null)
      {
        // TODO Fumbbl Error
        return runningGames;
      }
      else if (IsError(result))
      {
        // TODO API Error
        return runningGames;
      }
      else
      {
        // Got the games
        runningGames = JsonSerializer.Deserialize<List<RunningGame>>(result);
      }
      // TODO fix this
      return runningGames;
    }
    #endregion

    public const string API_GET_TEAM_MATCHES = "https://fumbbl.com/api/team/matches/";
    public async Task<List<RecentMatch>> GetTeamMatches(int id)
    {
      string result = null;
      List<RecentMatch> recentMatches = new List<RecentMatch>();
      
      result = await CallFumbblApi(API_GET_TEAM_MATCHES + id);
    
      if (result == null)
      {
        return recentMatches;
      }
      else if (IsError(result))
      {
        return recentMatches;
      }
      else
      {
        recentMatches = JsonSerializer.Deserialize<List<RecentMatch>>(result);
      }

      return recentMatches;
    }

    public const string API_GET_SCHEDULED_MATCHES = "https://fumbbl.com/api/tournament/schedule/";
    public async Task<List<ScheduledMatch>> GetScheduledMatches(int id)
    {
      string result = null;
      List<ScheduledMatch> recentMatches = new List<ScheduledMatch>();

      result = await CallFumbblApi(API_GET_SCHEDULED_MATCHES + id);

      if (result == null)
      {
        return recentMatches;
      }
      else if (IsError(result))
      {
        return recentMatches;
      }
      else
      {
        recentMatches = JsonSerializer.Deserialize<List<ScheduledMatch>>(result);
      }

      return recentMatches;
    }
  }
}
