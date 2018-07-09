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
            Control.Logger.LogDebug($"ML_InstallComponent_Default {order.MechComponentRef.ComponentDefID} - {order.MechComponentRef.Def == null}");
            if (!order.IsMechLabComplete)
                return;
            Control.Logger.LogDebug($"- search replace for {order.MechComponentRef.ComponentDefID}");
            if (order.MechComponentRef.Def is IDefaultRepace replace && order.PreviousLocation != ChassisLocations.None)
            {
                Control.Logger.LogDebug($"- found, adding {replace.DefaultID} to {order.PreviousLocation}");
                DefaultHelper.AddDefault(replace.DefaultID, __instance.GetMechByID(order.MechID), order.PreviousLocation, order.MechComponentRef.ComponentDefType);
            }
        }

    }
}
