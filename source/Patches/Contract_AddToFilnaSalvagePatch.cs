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
            if (def.ComponentType == ComponentType.MechPart)
                return true;

            if (def.MechComponentDef == null)
            {
                Control.Logger.Log($"AddToFinalSalvage: Def is null {def}");
                return true;
            }

            return !(def.MechComponentDef.Is<Flags>(out var f) && f.NotSalvagable);
        }
    }

    [HarmonyPatch(typeof(Contract), "AddMechComponentToSalvage")]
    internal static class Contract_AddMechComponentToSalvage
    {
        public static bool Prefix(MechComponentDef def)
        {
            Control.Logger.LogDebug(def.Description.Id);
            return !(def.Is<Flags>(out var f) && f.NotSalvagable);
        }
    }
}