using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_ML_InstallComponent_Patch_IAutoLinked
    {
        public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance)
        {
            Control.Logger.LogDebug($"ML_InstallComponent_IAutoLinked {order.MechComponentRef.ComponentDefID}");
            if (!order.IsMechLabComplete)
                return;


            if (!(order.MechComponentRef.Def is IAutoLinked linked) || linked.Links == null || linked.Links.Length == 0)
                return;

            Control.Logger.LogDebug("- is linked, proceed");

            var mech = __instance.GetMechByID(order.MechID);

            if (order.PreviousLocation != ChassisLocations.None)
                foreach(var link in linked.Links)
                {
                    Control.Logger.LogDebug($"- removing {link.ApendixID} from  {link.Location}");
                    DefaultHelper.RemoveDefault(link.ApendixID, mech, link.Location, link.BaseType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in linked.Links)
                {
                    Control.Logger.LogDebug($"- adding {link.ApendixID} to  {link.Location}");
                    DefaultHelper.AddDefault(link.ApendixID, mech , link.Location, link.BaseType, __instance);
                }
        }

    }
}
