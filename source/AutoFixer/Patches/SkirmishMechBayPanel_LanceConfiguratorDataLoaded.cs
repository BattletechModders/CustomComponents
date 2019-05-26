using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "LanceConfiguratorDataLoaded")]
    public static class SkirmishMechBayPanel_LanceConfiguratorDataLoaded
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void FixDefaults(SkirmishMechBayPanel __instance)
        {
            try
            {
                var mechDefs = __instance.dataManager.MechDefs.Select(pair => pair.Value).ToList();
                AutoFixer.Shared.FixMechDef(mechDefs);


                if(Control.Settings.DEBUG_DumpMechDefs && System.IO.Directory.Exists(Control.Settings.DEBUG_MechDefsDir))
                    foreach (var mechDef in mechDefs)
                    {
                        string str = mechDef.ToJSON();
                        FileStream fs = new FileStream(System.IO.Path.Combine(Control.Settings.DEBUG_MechDefsDir, $"{mechDef.Description.Id}.json"), FileMode.Create);
                        var sw = new StreamWriter(fs);
                        sw.Write(str);
                        sw.Flush();
                        fs.Close();
                    }

            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}
