using BattleTech.UI;
using Harmony;
using System.Linq;

namespace CustomComponents.Category
{
    internal static class RemoveController
    {
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    internal static class MechLabLocationWidget_OnItemGrab_Patch
    {

        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab)
        {
            Control.Logger.LogDebug($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            if (item.ComponentRef.Def is ICategory cat_item && !cat_item.CategoryDescriptor.AllowRemove && cat_item.CategoryDescriptor.MinEquiped > 0)
            {
                Control.Logger.LogDebug($"Custom found: Category: {cat_item.CategoryID}, checking if can remove");

                var count = ___mechLab.activeMechDef.Inventory
                    .Select(i => i.Def)
                    .OfType<ICategory>().Count(i => i.CategoryID == cat_item.CategoryID);

                Control.Logger.LogDebug($"Found {count} / {cat_item.CategoryDescriptor.MinEquiped}");

                if (count <= cat_item.CategoryDescriptor.MinEquiped)
                {
                    Control.Logger.LogDebug("Cancel!");
                    __result = false;
                    return false;
                }
                Control.Logger.LogDebug("Continue!");
            }
            return true;
        }

        public static void Postfix(ref bool __result)
        {
            Control.Logger.LogDebug($"OnItemGrab.Postfix CanRemove: {__result}");
        }
    }
}
