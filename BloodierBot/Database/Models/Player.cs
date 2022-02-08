using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class Player : IGetWithId
  {
    public string ApiGetByIdLink { get; set; } = "https://fumbbl.com/api/player/get/";

    public int id { get; set; }
    public int teamId { get; set; }
    public string status { get; set; }
    public int number { get; set; }
    public string name { get; set; }
    [JsonPropertyName("position")]
    public object positionGeneric 
    { 
      get
      {
        return position;
      }
      set
      {
        if (value is IdNamePair)
        {
          position = value as IdNamePair;
        }
        else
        {
          positionName = value as string;
        }
      }
    }
    [JsonIgnore]
    public IdNamePair position { get; set; }
    public string positionName { get; set; }
    public int positionId { get ; set; }
    //[JsonPropertyName("")]
    //public object positionGeneric { get; set; }
    public string gender { get; set; }
    public Stats stats { get; set; }
    public string portrait { get; set; }
    public string icon { get; set; }
    public Statistics statistics { get; set; }
    public List<string> skills { get; set; }
    [JsonPropertyName("injuries")]
    public object injuriesGeneric 
    { 
      get
      {
        return injuries;
      }
      set
      {
        if (value is List<String>)
        {
          injuries = value as List<String>;
        }
        else
        {
          injuriesString = value as string;
        }
      } 
    }
    [JsonIgnore]
    public List<string> injuries { get; set; }
    public string injuriesString { get; set; }

    public class Stats
    {
      public int ma { get; set; }
      public int st { get; set; }
      public int ag { get; set; }
      public int pa { get; set; }
      public int av { get; set; }
    }

    public class Statistics
    {
      public int spp { get; set; }
      public int completions { get; set; }
      public int touchdowns { get; set; }
      public int interceptions { get; set; }
      public int casualties { get; set; }
      public int mvp { get; set; }
      public int passing { get; set; }
      public int rushing { get; set; }
      public int blocks { get; set; }
      public int fouls { get; set; }
      public int games { get; set; }
    }
  }
}
