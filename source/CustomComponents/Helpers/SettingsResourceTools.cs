using System;
using System.Collections.Generic;
using System.IO;
using BattleTech;
using HBS.Util;

namespace CustomComponents;

public static class SettingsResourcesTools
{
    private class SettingsResource<T>
    {
#pragma warning disable 649
        public List<T> Settings;
#pragma warning restore 649
    }

    public static IEnumerable<T> Enumerate<T>(
        string resourceType,
        Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
    {
        if (customResources == null)
        {
            Log.CustomResource.Trace?.Log("customResources not found");
            yield break;
        }

        if (!customResources.TryGetValue(resourceType, out var entries))
        {
            Log.CustomResource.Trace?.Log($"{resourceType} not found");
            yield break;
        }

        foreach (var entry in entries.Values)
        {
            var settings = new SettingsResource<T>();
            try
            {
                Log.CustomResource.Trace?.Log($"Reading {entry.FilePath}");
                using (var reader = new StreamReader(entry.FilePath))
                {
                    var json = reader.ReadToEnd();
                    JSONSerializationUtility.FromJSON(settings, json);
                }
            }
            catch (Exception e)
            {
                Log.CustomResource.Trace?.Log($"Couldn't read {entry.FilePath}", e);
            }

            if (settings.Settings == null)
            {
                Log.CustomResource.Trace?.Log($"Settings is null in {entry.FilePath}");
                continue;
            }

            Log.CustomResource.Trace?.Log($" - total {settings.Settings.Count} entries loaded");

            foreach (var settingsEntry in settings.Settings)
            {
                yield return settingsEntry;
            }
        }
    }


}