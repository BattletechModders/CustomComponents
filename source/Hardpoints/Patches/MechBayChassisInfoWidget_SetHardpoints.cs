using System;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using UnityEngine;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechBayChassisInfoWidget))]
    [HarmonyPatch("SetHardpoints")]
    public static class MechBayChassisInfoWidget_SetHardpoints
    {
        [HarmonyPrefix]
        public static bool SetHardpoints(MechBayChassisInfoWidget __instance, LocalizableText ___jumpjetHardpointText,
            LocalizableText ___ballisticHardpointText, ChassisDef ___selectedChassis)
        {

            try
            {
                var hardpoints = __instance.GetComponent<UIModuleHPHandler>();
                if (hardpoints == null)
                {
                    hardpoints = __instance.gameObject.AddComponent<UIModuleHPHandler>();
                    hardpoints.Init(__instance, ___ballisticHardpointText.gameObject,
                        ___jumpjetHardpointText.gameObject, new Vector2(320,-25));
                }

                var usage = ___selectedChassis.GetHardpoints();
                hardpoints.SetDataTotal(usage);
                hardpoints.SetJJ(___selectedChassis);

            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
            return false;
        }
    }
}