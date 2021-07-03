using System;
using BattleTech;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using UnityEngine;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(TooltipPrefab_Mech))]
    [HarmonyPatch("SetData")]
    public static class TooltipPrefab_Mech_SetData
    {
        [HarmonyPostfix]
        public static void SetHardpoints(object data, TooltipPrefab_Mech __instance, LocalizableText ___JumpjetHP)
        {
            try
            {
                //Control.Log("tooltip mech");
                var handler = __instance.GetComponent<TooltipHPHandler>();
                if (handler == null)
                {
                    //Control.Log("creating");
                    handler = __instance.gameObject.AddComponent<TooltipHPHandler>();
                    handler.Init(__instance, ___JumpjetHP.transform.parent.gameObject);
                }

                var mech = data as MechDef;
                if (mech != null)
                {
                    //Control.Log($"set data for {mech.Description.Id}");
                    var usage = mech.GetHardpointUsage();
                    handler.SetData(usage);
                    handler.SetJJ(mech.GetJJCountByMechDef(), mech.GetJJMaxByMechDef());

                }
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(TooltipPrefab_Mech))]
    [HarmonyPatch("SetHardpointData")]
    public static class TooltipPrefab_Mech_SetHardpointData
    {
        [HarmonyPrefix]
        public static bool SetHardpoints(TooltipPrefab_Mech __instance)
        {
            return false;
        }
    }

}