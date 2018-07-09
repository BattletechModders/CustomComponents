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
            if (!order.IsMechLabComplete)
                return;

            if (!(order.MechComponentRef is IAutoLinked linked) || linked.Links == null || linked.Links.Length == 0)
                return;

            var mech = __instance.GetMechByID(order.ID);

            if (order.PreviousLocation != ChassisLocations.None)
                foreach(var link in linked.Links)
                {
                    DefaultHelper.RemoveDefault(link.ApendixID, mech, link.Location, link.BaseType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in linked.Links)
                {
                    DefaultHelper.AddDefault(link.ApendixID, mech , link.Location, link.BaseType);
                }
        }

    }
}
