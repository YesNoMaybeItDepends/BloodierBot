using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BloodierBot.Services
{
  public class Fumbbl
  {
    public const string API_GET_LIVE_GAMES = "https://fumbbl.com/api/match/current";
    public const string API_GET_MATCH_INFO = "https://fumbbl.com/api/match/get/3725357";

    public void GetMatchInfo()
    {
      using (WebClient webclient = new WebClient())
      {
        string html = webclient.DownloadString(API_GET_MATCH_INFO);
        var memes = JsonSerializer.Deserialize<RecentMatch>(html);
        Console.WriteLine(memes.team1.name);
      }
    }

    public void GetLiveGames()
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
  }

  /// boxtrophy

  // coach

  // group

  // match

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
}
