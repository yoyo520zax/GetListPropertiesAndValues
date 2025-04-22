using System;
using System.Collections.Generic;
using System.Data;
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
            new Person { Name = "Alice", Privilege = "Admin", System = "FAS", Age = 25, Address = "ABC" },
            new Person { Name = "Alice", Privilege = "User", System = "AD", Age = 25 },
            new Person { Name = "Bob", Privilege = "User", System = "AD", Age = 30, Address = "CDE" }
        };

        PrintListPropertiesAndValues(people);
        PrintUpdateQueries(people, new List<string> { "Name", "Privilege", "System" });
    }

    public static void PrintListPropertiesAndValues<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Console.WriteLine("List is empty.");
            return;
        }

        // 獲取類型的屬性
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        StringBuilder insertBuilder = new StringBuilder();

        // 儲存參數到字典
        var parameterDictionary = new Dictionary<string, object>();
        int cnt = 0;

        foreach (var item in list)
        {
            cnt++;

            StringBuilder colBuilder = new StringBuilder();
            StringBuilder valueBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                object propValue = property.GetValue(item);
                string columnName = property.Name;

                if (cnt.Equals(1)) 
                {
                    // 只在第一個物件時加入欄位名稱                    
                    colBuilder.Append($"{columnName}, ");
                }

                valueBuilder.Append($"@{property.Name}_{cnt}, ");
                parameterDictionary.Add($"@{property.Name}_{cnt}", propValue ?? "");
            }

            // 移除多餘的逗號與 AND
            string columnClause = colBuilder.ToString().TrimEnd(',', ' ');
            string valueClause = valueBuilder.ToString().TrimEnd(',', ' ');
            string tableName = type.Name;

            if (cnt.Equals(1))
            {
                // 只在第一個物件時加入 INSERT 語句
                insertBuilder.AppendLine($"INSERT {tableName}({columnClause}) VALUES ({valueClause}) ");
            }
            else
            {
                // 其他物件時加入逗號分隔
                insertBuilder.Append($", ({valueClause})");
            }
        }
        insertBuilder.Append(';');

        // 加入參數
        foreach (var kvp in parameterDictionary)
        {
            Console.WriteLine($"KEY '{kvp.Key}', VALUE '{kvp.Value}'");
        }

        Console.WriteLine(insertBuilder.ToString());
    }


    public static void PrintUpdateQueries<T>(List<T> list, List<string> primaryKeyNames)
    {
        if (list == null || list.Count == 0)
        {
            Console.WriteLine("List is empty.");
            return;
        }

        if (primaryKeyNames == null || primaryKeyNames.Count == 0)
        {
            Console.WriteLine("Primary keys are required.");
            return;
        }

        // 獲取類型的屬性
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();
        StringBuilder updateBuilder = new StringBuilder();
        // 儲存參數到字典
        var parameterDictionary = new Dictionary<string, object>();
        int cnt = 0;

        foreach (var item in list)
        {
            cnt++;

            StringBuilder setBuilder = new StringBuilder();
            StringBuilder whereBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                object propValue = property.GetValue(item);

                if (primaryKeyNames.Contains(property.Name))
                {
                    // 主鍵加入 WHERE 條件
                    string columnName = property.Name;
                    whereBuilder.Append($"{columnName} = @{property.Name}_{cnt} AND ");
                    parameterDictionary.Add($"@{property.Name}_{cnt}", propValue ?? DBNull.Value);
                }
                else
                {
                    // 排除值為 null 的欄位
                    if (propValue != null)
                    {
                        // 非主鍵屬性加入 SET 子句
                        string columnName = property.Name;
                        setBuilder.Append($"{columnName} = @{property.Name}_{cnt}, ");
                        parameterDictionary.Add($"@{property.Name}_{cnt}", propValue);
                    }
                }
            }

            // 移除多餘的逗號與 AND
            string setClause = setBuilder.ToString().TrimEnd(',', ' ');
            string whereClause = whereBuilder.ToString().TrimEnd(' ', 'A', 'N', 'D');
            string tableName = type.Name;

            updateBuilder.AppendLine($"UPDATE {tableName} SET {setClause} WHERE {whereClause};");
        }

        // 加入參數
        foreach (var kvp in parameterDictionary)
        {
            Console.WriteLine($"KEY '{kvp.Key}', VALUE '{kvp.Value}'");
        }

        Console.WriteLine(updateBuilder.ToString());
    }
}



public class Person
{
    public string Name { get; set; }
    public string Privilege { get; set; }
    public string System { get; set; }
    public int Age { get; set; }
    public string Address { get; set; }
}
