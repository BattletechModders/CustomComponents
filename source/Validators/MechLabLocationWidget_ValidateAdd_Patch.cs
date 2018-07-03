using System;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "ValidateAdd", new Type[] {typeof(MechComponentDef)})]
    internal static class MechLabLocationWidget_ValidateAdd_Patch
    {
        internal static void Postfix(MechComponentDef newComponentDef,
            MechLabLocationWidget __instance, ref bool __result, ref string ___dropErrorMessage,
            MechLabPanel ___mechLab
        )
        {
            __result = Validator.ValidateAdd(newComponentDef, __instance, __result, ref ___dropErrorMessage,
                ___mechLab);


            if (newComponentDef is IValidateAdd)
            {
                __result = (newComponentDef as IValidateAdd).ValidateAdd(__instance, __result, ref ___dropErrorMessage,
                    ___mechLab);
            }
        }
    }
}