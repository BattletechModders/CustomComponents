using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_ML_InstallComponent_Patch
    {
        public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance)
        {
            try
            {
                Control.LogDebug(DType.ComponentInstall, $"ML_InstallComponent {order.MechComponentRef.ComponentDefID} - {order.MechComponentRef.Def == null}");
                if (!order.IsMechLabComplete)
                    return;

                var mech = __instance.GetMechByID(order.MechID);
                if (mech == null)
                    return;

                foreach (var handler in order.MechComponentRef.GetComponents<IOnInstalled>())
                {
                    handler.OnInstalled(order, __instance, mech);
                }
                Control.LogDebug(DType.ComponentInstall, $"ML_InstallComponent complete");

            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }

    }
}
