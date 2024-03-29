﻿using Dapper;
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

    public async Task<bool> DeleteUser(IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", Id);
      bool result = false;
      try
      {
        result = await db.ExecuteAsync(Properties.Resources.deleteUser, p) == 1 ? true : false;
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

    public static async Task<int?> getMoney(ulong userId, IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", userId);
      int? money = null;
      try
      {
        money = await db.QuerySingleAsync<int?>(Properties.Resources.selectMoneyFromUser,p);
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return money;
    }

    /// <summary>
    /// DOES NOT WORK, FIXME
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<int?> getMoney(IDbConnection db)
    {
      DynamicParameters p = new DynamicParameters();
      p.Add("Id", Id);
      int? money = await db.QuerySingleOrDefault(Properties.Resources.selectMoneyFromUser, p);
      return money;
    }
  }
}
