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
            if (!order.IsMechLabComplete)
                return;

            if (!(order.MechComponentRef is ICategory category) || !category.CategoryDescriptor.AutoReplace
                || (category.CategoryDescriptor.MaxEquiped < 0 && category.CategoryDescriptor.MaxEquipedPerLocation < 0))
                return;

            if (order.DesiredLocation == ChassisLocations.None)
                return;

            var mech = __instance.GetMechByID(order.ID);



            int n1 = mech.Inventory.Count(i => i.Def is ICategory cat && category.CategoryID == cat.CategoryID);
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.Def is ICategory cat
                   && category.CategoryID == cat.CategoryID);

            var replace = mech.Inventory.FirstOrDefault(i => i.MountedLocation == order.DesiredLocation && i.Def is ICategory cat
                   && category.CategoryID == cat.CategoryID && i.Def is IDefault);

            if (replace == null)
                return;

            bool need_replace = (category.CategoryDescriptor.MaxEquiped > 0 && n1 > category.CategoryDescriptor.MaxEquiped) ||
                (category.CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > category.CategoryDescriptor.MaxEquipedPerLocation);

            if (need_replace)
                DefaultHelper.RemoveDefault(replace.ComponentDefID, mech, order.DesiredLocation, replace.ComponentDefType);
            
        }

    }
}
