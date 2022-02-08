using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BloodierBot.Services;
using BloodierBot.Database.Models;
using AutoMapper.Configuration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using System.Data;
using System.Data.SQLite;

namespace BloodierBot.Database.Models
{
  public class RunningGame : IEquatable<RunningGame>
  {
    [JsonPropertyName("id")]
    public int RunningGame_Id { get; set; }
    public int half { get; set; }
    public int turn { get; set; }
    public string division { get; set; }
    public Tournament? tournament { get; set; } = null;
    public List<RunningGameTeam> teams { get; set; }

    // Why is it equatable with itself?
    public bool Equals(RunningGame? other)
    {
      if (other is null)
        return false;

      return RunningGame_Id == other.RunningGame_Id;
    }
    public override bool Equals(object? obj) => Equals(obj as RunningGame);
    public override int GetHashCode() => (RunningGame_Id).GetHashCode();

    public static async Task<List<RunningGame>> GetRunningGames()
    {
      var fumbblapi = new FumbblApi();
      var livegames = await fumbblapi.GetRunningGames();
      return livegames;
    }

    public async Task<bool> DbInsertRunningGame(IDbConnection db)
    {
      int success = 0;
      bool result = false;

      // Insert Running Game
      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("Id", RunningGame_Id);
      parameters.Add("Half", half);
      parameters.Add("Turn", turn);
      parameters.Add("Division", division);
      if (tournament != null)
      {
        parameters.Add("TournamentId", tournament.Tournament_Id);
      }
      else
      {
        parameters.Add("TournamentId", null);
      }
      success += await db.ExecuteAsync(Properties.Resources.insertRunningGame, parameters);

      // Insert Tournament
      if (tournament != null)
      {
        await tournament.DbInsertTournament(db);
      }

      // Insert Teams
      foreach (var team in teams)
      {
        team.RunningGameId = RunningGame_Id;
        team.DbInsertRunningGameTeam(db);
      }
      
      result = (success == 4) ? true : false;
      return result;
    }

    public static List<RunningGame> GetRunningGamesFromDatabase(IDbConnection db)
    {
        var gamesById = new Dictionary<int, RunningGame>();
        db.Query<RunningGame, Tournament, RunningGameTeam, RunningGame>(Properties.Resources.selectRunningGames,
          (g, tt, t) =>
          {
            RunningGame game;
            if (!gamesById.TryGetValue(g.RunningGame_Id, out game))
            {
              gamesById.Add(g.RunningGame_Id, game = g);
            }
            if (game.teams == null)
            {
              game.teams = new List<RunningGameTeam>();
            }
            game.teams.Add(t);
            if (tt != null)
            {
              game.tournament = tt;
            }
            return game;
          }, splitOn: "RunningGame_Id,Tournament_Id,RunningGameTeam_Id").AsQueryable();

        return gamesById.Values.ToList();
    }

    public int DeleteRunningGame(IDbConnection db)
    {
      DynamicParameters args = new DynamicParameters();
      args.Add("RunningGameId", RunningGame_Id);

      int deletedTeams = db.Execute(Properties.Resources.DeleteRunningGameTeams, args);
      int deletedGames = db.Execute(Properties.Resources.DeleteRunningGame, args);
      return (deletedGames + deletedTeams);
    }
  }
}