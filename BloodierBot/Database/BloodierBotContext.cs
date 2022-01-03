using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BloodierBot.Database.Models;

namespace BloodierBot.Database
{
  public partial class BloodierBotContext : DbContext
  {
    public virtual DbSet<EightBallAnswer> EightBallAnswers { get; set;}
    public virtual DbSet<RunningGame> RunningGames { get; set;}
    public virtual DbSet<RunningGameTeam> RunningGameTeams { get; set;}
    public virtual DbSet<RunningGameTournament> RunningGameTournaments { get; set; }
    //public virtual DbSet<Team> Teams { get; set; }
    //public virtual DbSet<Tournament> Tournaments { get; set;}
    public string connectionString = @"name=Default;Data Source=bloodierbot.db;Version=3; providerName=System.Data.Client";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "bloodierbot.db" };
      //var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = @"C:\Users\me\dev\BloodierBot\BloodierBot\bloodierbot.db" };
      var connectionString = connectionStringBuilder.ToString();
      var connection = new SqliteConnection(connectionString);
      optionsBuilder.UseSqlite(connection);
    }
  }
}
