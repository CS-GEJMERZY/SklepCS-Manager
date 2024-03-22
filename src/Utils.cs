using System.Reflection;
using CounterStrikeSharp.API.Modules.Utils;

namespace SklepCSManager;

public partial class SklepcsManagerPlugin
{
    public void PreparePluginPrefix()
    {
        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";

            if (PluginChatPrefix.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                PluginChatPrefix = PluginChatPrefix.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public List<string> GetLines(string message)
    {
        List<string> result = new();
        string[] lines = message.Split('\n');

        result = lines.ToList();

        return result;
    }
}