using BloodierBot.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DapperExtensions;

namespace BloodierBot.Database.Models
{
  public class Tournament : IGetWithId, IEquatable<RunningGameTournament>
  {
    public string ApiGetByIdLink { get; set; } = "https://fumbbl.com/api/tournament/get/";
    //public const API_GET_GROUP_TOURNAMENTS = ""

    [JsonConverter(typeof(TournamentIdConverter))]
    [JsonPropertyName("id")]
    [Column("Id")]
    public int Tournament_Id { get; set; }
    // Why is it t_name?
    [JsonPropertyName("name")]
    [Column("Name")]
    public string t_name { get; set; } = "poop";
    public string type { get; set; }
    public string status { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string season { get; set; }
    public IdNamePair winner { get; set; }
    public int? group = null;

    private class dbmeme
    {
      public int Id { get; set; }
      public string name { get; set; }
    }

    public static async Task<Tournament> ApiGetById(int id)
    {
      Tournament? t = null;
      FumbblApi fapi = new FumbblApi();
      t = await fapi.GetThing<Tournament>(id) as Tournament;
      return t;
    }

    public async Task<bool> DbInsertTournament(IDbConnection db)
    {
      DynamicParameters parameters = new DynamicParameters();
      parameters.Add("Id", Tournament_Id);
      parameters.Add("Name", t_name);
      int rows = await db.ExecuteAsync(Properties.Resources.insertTournament, parameters);
      bool result = rows > 0 ? true : false;
      return result;
    }

    public static async Task<List<Tournament>> DbSelectAllTournaments(IDbConnection db)
    {
      List<Tournament>? list = new List<Tournament>();
      //list = await db.QueryAsync<Tournament>(Properties.Resources.selectAllTournaments) as List<Tournament>;
      var stuff = await db.QueryAsync<dbmeme>(Properties.Resources.selectAllTournaments);
      stuff.ToList();
      foreach (dbmeme lol in stuff)
      {
        list.Add(new Tournament { t_name = lol.name, Tournament_Id = lol.Id }) ;
      }
      //list = stuff.ToList();
      return list;
      
    }

    public bool Equals(RunningGameTournament? t)
    {
      return (Tournament_Id == t.RunningGameTournament_Id);
    }
  }

  public class TournamentIdConverter : JsonConverter<int>
  {
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
          case JsonTokenType.String:
            if (int.TryParse(reader.GetString(), out int result))
            {
              return result;
            }
            else
              return -1;
          case JsonTokenType.Number:
            return reader.GetInt32();
          default:
            Console.WriteLine("BOB SAGGET");
            return -1;
        }
      return -1;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
      JsonSerializer.Serialize(writer, value);
    }
  }
}
