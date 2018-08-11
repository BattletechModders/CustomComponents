using System;
using BattleTech;
using Harmony;
using HBS.Logging;

namespace CustomComponents
{ 
    [HarmonyPatch(typeof(Contract), "AddToFinalSalvage")]
    internal static class Contract_AddToFilnaSalvagePatch
    {
        public static bool Prefix(SalvageDef def)
        {
            var flags = def.MechComponentDef?.GetComponent<Flags>();

            //Control.Logger.LogDebug($"AddToFinalSalvage: {def.Description.Id}, Salvagable:{flags == null || !flags.NotSalvagable}");

            return flags == null || !flags.NotSalvagable;
        }
    }

    [HarmonyPatch(typeof(Contract), "AddMechComponentToSalvage")]
    internal static class Contract_AddMechComponentToSalvage
    {
        public static bool Prefix(MechComponentDef def)
        {
            var flags = def.GetComponent<Flags>();
            //Control.Logger.LogDebug($"salvage: {def.Description.Id}   Flags null:{flags == null}  Default:{flags!= null && flags.Default}  Salvagabe:{flags != null && flags.NotSalvagable}");
            return !(flags!= null && flags.NotSalvagable);
        }
    }
}