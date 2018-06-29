using DynModLib;
using Harmony;
using System;
using System.Reflection;

namespace CustomComponents
{
    public static class Control
    {
        internal static Mod mod;

        public static void Init(string directory, string settingsJSON)
        {
            mod = new Mod(directory);
            try
            {
                //   mod.LoadSettings(settings);

                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                // logging output can be found under BATTLETECH\BattleTech_Data\output_log.txt
                // or also under yourmod/log.txt
                mod.Logger.Log("Loaded " + mod.Name);
            }
            catch (Exception e)
            {
                mod.Logger.LogError(e);
            }
        }
    }
}
