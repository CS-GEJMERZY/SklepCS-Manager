using System.Reflection;
using CounterStrikeSharp.API.Modules.Utils;

namespace SklepCSManager;

public partial class SklepcsManagerPlugin
{
    public void PreparePrefixColor()
    {
        string result = Config.Settings.Prefix;
        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";

            if (result.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
        Config.Settings.Prefix = result;
    }


    public List<string> GetLines(string message)
    {
        List<string> result = new();
        string[] lines = message.Split('\n');

        result = lines.ToList();

        return result;
    }
}