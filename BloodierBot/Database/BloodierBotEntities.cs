using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BloodierBot.Database
{
  public partial class BloodierBotEntities : DbContext
  {
    public virtual DbSet<EightBallAnswer> EightBallAnswer { get; set;}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "bloodierbot.db" };
      var connectionString = connectionStringBuilder.ToString();
      var connection = new SqliteConnection(connectionString);
      optionsBuilder.UseSqlite(connection);
    }
  }
}
