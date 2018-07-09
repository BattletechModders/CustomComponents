using BattleTech;
using Harmony;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_ML_InstallComponent_Patch_Category
    {
        public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance)
        {
            Control.Logger.LogDebug($"ML_InstallComponent_ICategory for {order.MechComponentRef.ComponentDefID} {order.MechComponentRef.Def == null}");
            if (!order.IsMechLabComplete)
                return;


            if (!(order.MechComponentRef.Def is ICategory category) || !category.CategoryDescriptor.AutoReplace
                                                                || (category.CategoryDescriptor.MaxEquiped < 0 &&
                                                                    category.CategoryDescriptor.MaxEquipedPerLocation <
                                                                    0))
            {
                if (order.MechComponentRef.Def is ICategory c)
                {
                    var c1 = c.CategoryDescriptor;
                    Control.Logger.LogDebug($"- {c1.DisplayName} r:{c1.AutoReplace}  max:{c1.MaxEquiped} mpr:{c1.MaxEquipedPerLocation} = not requre replace");
                }
                else
                    Control.Logger.LogDebug("-  not category");
                return;
            }


            if (order.DesiredLocation == ChassisLocations.None)
            {
                Control.Logger.LogDebug("- removing, no additional actions");
                return;
            }

            var mech = __instance.GetMechByID(order.MechID);
            

            int n1 = mech.Inventory.Count(i => i.Def is ICategory cat && category.CategoryID == cat.CategoryID);
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.Def is ICategory cat
                   && category.CategoryID == cat.CategoryID);

            Control.Logger.LogDebug($"- total {n1}/{category.CategoryDescriptor.MaxEquiped}  location: {n2}/{category.CategoryDescriptor.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => i.MountedLocation == order.DesiredLocation && i.Def is ICategory cat
                   && category.CategoryID == cat.CategoryID && i.Def is IDefault);

            Control.Logger.LogDebug($"- possible replace: {(replace == null? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (category.CategoryDescriptor.MaxEquiped > 0 && n1 > category.CategoryDescriptor.MaxEquiped) ||
                (category.CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > category.CategoryDescriptor.MaxEquipedPerLocation);

            Control.Logger.LogDebug($"- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveDefault(replace.ComponentDefID, mech, order.DesiredLocation, replace.ComponentDefType);
            
        }

    }
}
