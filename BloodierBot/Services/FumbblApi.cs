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

    public async Task<string?> CallFumbblApi(string query)
    {
      string? result = null;
      Uri uri = new Uri(query);

      try
      {
        using (var webclient = new WebClient())
        {
          // Handle Download Completed Event
          webclient.DownloadStringCompleted += (sender, e) =>
          {
            try
            {
              if (e != null && e.Result != null)
              {
                result = e.Result;
              }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
          };

          // Download the resource
          result = await webclient.DownloadStringTaskAsync(uri);
        }
      }
      catch (WebException ex)
      {
        // TODO log
        Console.WriteLine("ERROR -> " + ex.ToString());
      }

      return result;
    }

    public async Task<RecentMatch> GetRecentMatch(int id)
    {
      string? result = null;
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

    public const string API_GET_COACH_INFORMATION = "https://fumbbl.com/api/coach/get/";

    #endregion

    #region Group Section
    #endregion

    #region match

    public const string API_GET_LIVE_GAMES = "https://fumbbl.com/api/match/current";
    
    public async Task<List<RunningGame>> GetRunningGames()
    {
      string? result = null;
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
      string? result = null;
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

    // TODO handle errors better
    public async Task<IGetWithId> GetThing<T>(int id) where T : IGetWithId, new()
    {
      //string url = "https://fumbbl.com/api/player/get/";
      //string url = "";

      string? result = null;
      T thing = new T();

      result = await CallFumbblApi(thing.ApiGetByIdLink + id);

      if (result == null)
      {
        return null;
      }
      else if (IsError(result))
      {
        return null;
      }
      else
      {
        thing = JsonSerializer.Deserialize<T>(result);
        return thing;
      }
    }
  }
}
