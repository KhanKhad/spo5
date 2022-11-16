using System.Linq;
using System.Text.RegularExpressions;

internal class Program
{
    public static List<FileString> _file;
    private async static Task Main(string[] args)
    {
        _file = new List<FileString>();

        var file = await ReadIdentificators(@"C:\Users\xxano\Desktop\ghj\6.txt");
        foreach (var item in file)
        {
            if (!GetTypes(item))
            {
                Console.WriteLine("Wrong File");
                return;
            }
        }
        var a = !CheckFile1Step(_file.ToList());
        var b = !CheckFile2Step(_file.ToList());
        var c = !CheckFile3Step(_file.ToList());
        if (a || b || c)
        {
            Console.WriteLine("Wrong File");
            return;
        }
        else
        {
            Console.WriteLine("File fits");
            return;
        }
    }

    private static bool CheckFile1Step(List<FileString> file)
    {
        if (file.Count == 0) return true;
        file = file.Where(i => i.Type != StringTypes.Condition).ToList();
        try
        {
            var array = file.Select(i=>i.Type).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == StringTypes.If && array[i+1] == StringTypes.LogicalCondition &&
                    array[i + 2] == StringTypes.Then)
                {
                    if (array.Length > 3 && array[i+3] == StringTypes.Else)
                    {
                        i += 3;
                    }
                    else
                    {
                        i += 2;
                    }
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private static bool CheckFile2Step(List<FileString> file)
    {
        if (file.Count == 0) return true;
        try
        {
            var fileItem = file.First();
            file.RemoveAt(0);
            switch (fileItem.Type)
            {
                case StringTypes.Condition:
                    return CheckFile2Step(file);
                case StringTypes.If:
                    if (file.First().Type != StringTypes.LogicalCondition) return false;
                    else file.RemoveAt(0);
                    return CheckFile2Step(file);
                case StringTypes.Then:
                    if (file.First().Type != StringTypes.Condition) return false;
                    else file.RemoveAt(0);
                    return CheckFile2Step(file);
                case StringTypes.Else:
                    if (file.First().Type != StringTypes.Condition) return false;
                    else file.RemoveAt(0);
                    return CheckFile2Step(file);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private static bool CheckFile3Step(List<FileString> file)
    {
        if (file.Count == 0) return true;
        var file1 = file.Where(i => i.Type == StringTypes.Condition).ToList();
        var file2 = file.Where(i => i.Type == StringTypes.LogicalCondition).ToList();
        try
        {
            foreach (var condition in file1)
            {
                if (!CheckCondition(condition.Value))
                {
                    return false;
                }
            }
            foreach (var logicCondition in file2)
            {
                if (!CheckLogicCondition(logicCondition.Value))
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private static bool CheckCondition(string input)
    {
        Regex regex = new Regex(@"[a-z]+:=[a-z]+", RegexOptions.IgnoreCase);
        return regex.IsMatch(input);
    }
    private static bool CheckLogicCondition(string input)
    {
        if (input.Contains('*')) return false;
        input = input.Replace("\"", "\'");
        Regex regex = new Regex(@"([a-z]+|'[a-z]+')(<|>|=)([a-z]+|'[a-z]+')", RegexOptions.IgnoreCase);
        return regex.IsMatch(input);
    }
    private static bool GetTypes(string input)
    {
        try
        {
            input = input.Trim();
            if (input.StartsWith("if "))
            {
                var condition = input.Substring(3);
                _file.Add(new FileString() { Value = input.Substring(0, 2), Type = StringTypes.If });
                _file.Add(new FileString() { Value = condition, Type = StringTypes.LogicalCondition });
                return true;
            }
            if (input.StartsWith("then ") && input.EndsWith(";"))
            {
                var condition = input.Substring(5);
                _file.Add(new FileString() { Value = input.Substring(0, 4), Type = StringTypes.Then });
                _file.Add(new FileString() { Value = condition, Type = StringTypes.Condition });

                return true;
            }
            if (input == "else")
            {
                _file.Add(new FileString() { Value = input, Type = StringTypes.Else });
                return true;
            }
            if (input.Contains(":=") && input.EndsWith(";"))
            {
                _file.Add(new FileString() { Value = input, Type = StringTypes.Condition });
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }


    private static async Task<List<string>> ReadIdentificators(string path)
    {
        var identificators = new List<string>();

        using (var reader = new StreamReader(path))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                identificators.Add(line);
            }
        }

        return identificators;
    }


}
public enum StringTypes
{
    Condition,
    If,
    LogicalCondition,
    Then,
    Else,
}
public class FileString
{
    public StringTypes Type { get; set; }
    public string Value { get; set; }
}