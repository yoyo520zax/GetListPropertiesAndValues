using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

public class Program
{
    public static void Main()
    {
        // 建立示範 List<T>，這裡以 Person 類型為例
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 30 }
        };

        PrintListPropertiesAndValues(people);
    }

    public static void PrintListPropertiesAndValues<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Console.WriteLine("List is empty.");
            return;
        }

        // 取得類型的屬性
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        string columns = "";
        foreach (var property in properties)
        {
            // 讀取屬性名稱
            string name = property.Name;
            columns += "'" + name + "',";
        }
        columns = columns.TrimEnd(',');

        string H = "INSERT INTO {0} ({1}) VALUES {2}";
        StringBuilder values = new StringBuilder();

        foreach (var item in list)
        {
            string value = "";
            foreach (PropertyInfo property in item.GetType().GetProperties())
            {
                value += "'" + property.GetValue(item) + "',";
                //Console.WriteLine($"  {property.Name}: {property.GetValue(item)}");
            }
            values.Append("(" + value.TrimEnd(',') + "),");
        }
        string val = values.ToString().TrimEnd(',');
        Console.WriteLine(string.Format(H, type.Name, columns, val));
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
