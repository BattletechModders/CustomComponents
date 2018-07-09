using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_ML_InstallComponent_Patch_Default
    {
        public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance)
        {
            if (!order.IsMechLabComplete)
                return;

            if(order.MechComponentRef.Def is IDefaultRepace replace && order.PreviousLocation != ChassisLocations.None)
            {
                DefaultHelper.AddDefault(replace.DefaultID, __instance.GetMechByID(order.ID), order.PreviousLocation, order.MechComponentRef.ComponentDefType);
            }
        }

    }
}
