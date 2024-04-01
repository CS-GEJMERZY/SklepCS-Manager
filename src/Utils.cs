using System.Reflection;
using CounterStrikeSharp.API.Modules.Utils;

namespace Plugin;

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

    public static List<string> GetLines(string message)
    {
        List<string> result = message.Split('\n').ToList();

        return result;
    }
}