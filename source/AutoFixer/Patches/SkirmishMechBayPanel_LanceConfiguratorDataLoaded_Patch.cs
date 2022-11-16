using System;
using System.IO;
using System.Linq;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SkirmishMechBayPanel), "LanceConfiguratorDataLoaded")]
public static class SkirmishMechBayPanel_LanceConfiguratorDataLoaded_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    public static void FixDefaults(SkirmishMechBayPanel __instance)
    {
        try
        {
            var mechDefs = __instance.dataManager.MechDefs.Select(pair => pair.Value).ToList();
            AutoFixer.Shared.FixMechDef(mechDefs);

            if (Control.Settings.DEBUG_DumpMechDefs && Directory.Exists(Control.Settings.DEBUG_MechDefsDir))
            {
                foreach (var mechDef in mechDefs)
                {
                    var str = mechDef.ToJSON();
                    using (var fs = new FileStream(Path.Combine(Control.Settings.DEBUG_MechDefsDir, $"{mechDef.Description.Id}.json"), FileMode.Create))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            sw.Write(str);
                            sw.Flush();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}