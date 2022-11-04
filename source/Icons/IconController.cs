using System;
using System.Collections.Generic;
using System.IO;
using BattleTech;
using SVGImporter;

namespace CustomComponents
{
    public static class IconController
    {
        private static Dictionary<string, SVGAsset> icons = new Dictionary<string, SVGAsset>();

        internal static SVGAsset Get(string resourceId) => icons.TryGetValue(resourceId, out var icon) ? icon : null;

        internal static bool Contains(string ressourceId) => icons.ContainsKey(ressourceId);

        internal static void LoadIcons(Dictionary<string, VersionManifestEntry> icons_ressourses)
        {
            Control.LogDebug(DType.Icons, $"Get data:");
            foreach (var pair in icons_ressourses)
            {
                Control.LogDebug(DType.Icons, $"- {pair.Key}: {pair.Value.FileName} {pair.Value.FilePath} {pair.Value.Name}");
                try
                {
                    using (var reader = new StreamReader(pair.Value.FilePath))
                    {
                        var txt = reader.ReadToEnd();
                        var icon = SVGAsset.Load(txt);
                        icons["@" + pair.Key] = icon;
                    }
                }
                catch(Exception e)
                {
                    Control.LogError(e);
                }
                
            }
        }
    }
}
