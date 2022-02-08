using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tests
{
  public static class Helpers
  {
    public static void GetAllProperties(object obj)
    {
      foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
      {
        string name = descriptor.Name;
        object value = descriptor.GetValue(obj);
        Console.WriteLine("{0} : {1}", name, value);
      }
    }

    public static void PrintObject(object obj)
    {
      //Console.WriteLine(obj.ToString());
      JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
      string serializedObject = JsonSerializer.Serialize(obj, options);
      Console.WriteLine(serializedObject);
    }
  }
}
