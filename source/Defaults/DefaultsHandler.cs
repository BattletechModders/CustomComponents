using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public class DefaultsHandler
    {
        internal static DefaultsHandler Shared = new DefaultsHandler();
        
        internal readonly List<DefaultsInfo> TaggedDefaults = new List<DefaultsInfo>();
        internal readonly List<DefaultsInfo> Defaults = new List<DefaultsInfo>();

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var entry in SettingsResourcesTools.Enumerate<DefaultsInfo>("CCDefaults", customResources))
            {
                if (string.IsNullOrEmpty(entry.Tag))
                {
                    Defaults.Add(entry);
                }
                else
                {
                    TaggedDefaults.Add(entry);
                }
            }
        }
    }
}