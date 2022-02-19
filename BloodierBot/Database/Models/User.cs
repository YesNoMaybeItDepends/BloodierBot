using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodierBot.Database.Models
{
  public class User
  {
    public ulong Id { get; set; }
    public string Name { get; set; }
    public int Money { get; set; }

    public User(ulong id, string name)
    {
      Id = id;
      Name = name;
    }

    public async Task<bool> RegisterUser(User user, IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", user.Id);
      p.Add("Name", user.Name);
      bool result = false;
      try
      {
        result = await db.ExecuteAsync(Properties.Resources.insertUser, p) == 1 ? true : false;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      return result;
    }

    public static async Task<bool> updateMoney(ulong userId, int money, IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", userId);
      p.Add("Money", money);
      bool result = false;
      try
      {
        result = await db.ExecuteAsync(Properties.Resources.updateUserMoney, p) == 1 ? true : false;
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex);
      }
      return result;
    }

    public async Task<bool> updateMoney(int money, IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", Id);
      p.Add("Money", money);
      bool result = false;
      try
      {
        result = await db.ExecuteAsync(Properties.Resources.updateUserMoney, p) == 1 ? true : false;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      return result;
    }
  }
}
