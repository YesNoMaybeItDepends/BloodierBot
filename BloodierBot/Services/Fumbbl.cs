using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BloodierBot.Services
{
  // TODO turn downloadstring into downloadstringasync
  public class Fumbbl
  {

    public const string API_GET_MATCH_INFO = "https://fumbbl.com/api/match/get/3725357";

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

    public void GetMatchInfo()
    {
      using (WebClient webclient = new WebClient())
      {
        string html = webclient.DownloadString(API_GET_MATCH_INFO);
        var memes = JsonSerializer.Deserialize<RecentMatch>(html);
        Console.WriteLine(memes.team1.name);
      }
    }

    public void GetLiveGamesssss()
    {
      using (WebClient webclient = new WebClient())
      {
        string html = webclient.DownloadString(API_GET_LIVE_GAMES);
        var memes = JsonSerializer.Deserialize<List<RunningGame>>(html);

        foreach (RunningGame game in memes)
        {
          Console.WriteLine($"{game.id}: {game.teams[0].name} vs {game.teams[1].name} ");
        }
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
    
    public async Task<List<RunningGame>> GetLiveGames()
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

    // oauth

    // player

    // playerimage

    // position

    // roster

    // ruleset

    // skill

    // stats

    // team

    // tournament
  }

  public class RunningGame
  {
    public int id { get; set; }
    public int half { get; set; }
    public int turn { get; set; }
    public string divison { get; set; }
    public Tournament tournament { get; set; }
    public Team[] teams { get; set; }
  }

  public class Tournament
  {
    public int id { get; set; }
    public int group { get; set; }
  }

  public class Team
  {
    public int id { get; set; }
    public string side { get; set; }
    public string name { get; set; }
    public string coach { get; set; }
    public string race { get; set; }
    public string rating { get; set; }
    public int score { get; set; }
    public string logo { get; set; }
  }

  public class Coach
  {
    public int id { get; set; }
    public string name { get; set; }
  }

  public class RecentMatch
  {
    public int id { get; set; }
    public int replayId { get; set; }
    public int tournamentId { get; set; }
    public string date { get; set; }
    public string time { get; set; }
    public int gate { get; set; }
    public string conceded { get; set; }
    public RecentMatchTeam team1 { get; set; }
    public RecentMatchTeam team2 { get; set; }
  }

  public class RecentMatchTeam
  {
    public int id { get; set; }
    public string name { get; set; }
    public int score { get; set; }
    public Casualties casualties { get; set; }
    public int fanfactor { get; set; }
    public string teamValue { get; set; }
    public Coach coach { get; set; }
    public int winnings { get; set; }
    public int gate { get; set; }
    public string tournamentWeight { get; set; }
  }

  public class Casualties
  {
    public int bh { get; set; }
    public int si { get; set; }
    public int rip { get; set; }
  }

  public class IdNamePair
  {
    public int id { get; set; }
    public string name { get; set; }
  }
}
