using BattleTech;
using BattleTech.UI;
using Harmony;
using System;

namespace CustomComponents.WeightLimitation
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "ValidateAdd", new Type[] { typeof(MechComponentDef) })]
    public static class MechLabLocationWidget_ValidateAdd_Patch111
    {
        public static void Postfix(MechComponentDef newComponentDef,
            MechLabLocationWidget __instance, ref bool __result, ref string ___dropErrorMessage,
            MechLabPanel ___mechLab
        )
        {
            if (__result)
                return;

            if(newComponentDef is IWeightLimited)
            {
                var limit = newComponentDef as IWeightLimited;

                if(___mechLab.activeMechDef.Chassis.Tonnage != limit.AllowedTonnage)
                {
                    __result = false;
                    ___dropErrorMessage = string.Format("{0} designed for {1}t mech", newComponentDef.Description.Name.ToUpper(), limit.AllowedTonnage);
                }

            }
        }
    }

}
