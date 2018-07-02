using System;
using BattleTech.UI;
using Harmony;
using System.Linq;
using BattleTech;
using BattleTech.Data;

namespace CustomComponents.Category
{
    internal static class RemoveController
    {
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    internal static class MechLabLocationWidget_OnItemGrab_Patch
    {

        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab, ref MechComponentRef __state)
        {
            Control.Logger.LogDebug($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            __state = null;

            if (!(item.ComponentRef.Def is ICategory cat_item) || cat_item.CategoryDescriptor.AllowRemove ||
                cat_item.CategoryDescriptor.MinEquiped <= 0) return true;


            Control.Logger.LogDebug($"Custom found: Category: {cat_item.CategoryID}, checking if can remove");

            var count = ___mechLab.activeMechDef.Inventory
                .Select(i => i.Def)
                .OfType<ICategory>().Count(i => i.CategoryID == cat_item.CategoryID);

            Control.Logger.LogDebug($"Found {count} / {cat_item.CategoryDescriptor.MinEquiped}");

            if (count <= cat_item.CategoryDescriptor.MinEquiped)
            {
                if (string.IsNullOrEmpty(cat_item.CategoryDescriptor.DefaultReplace) || cat_item.CategoryDescriptor.DefaultReplace == item.ComponentRef.ComponentDefID)
                {
                    Control.Logger.LogDebug("No DefaultReplace, cancel");
                    __result = false;
                    return false;
                }

                var component_ref = new MechComponentRef(cat_item.CategoryDescriptor.DefaultReplace, String.Empty, item.ComponentRef.ComponentDefType, ChassisLocations.None, -1, ComponentDamageLevel.Installing);
                component_ref.DataManager = ___mechLab.dataManager;
                component_ref.RefreshComponentDef();
                if (component_ref.Def == null)
                {
                    Control.Logger.LogDebug("Default replace not found, cancel");
                    __result = false;
                    return false;
                }
                Control.Logger.LogDebug("Default replace found");
                __state = component_ref;
            }

            Control.Logger.LogDebug("Continue!");
            return true;
        }

        public static void Postfix(ref bool __result, MechComponentRef __state, MechLabPanel ___mechLab, MechLabLocationWidget __instance)
        {

            Control.Logger.LogDebug($"OnItemGrab.Postfix CanRemove: {__result}");
            if (__state != null)
            {
                Control.Logger.LogDebug($"OnItemGrab.Postfix Replacement received: {__state.ComponentDefID}");

                try
                {
                    var slot = ___mechLab.CreateMechComponentItem(__state, false, __instance.loadout.Location,
                        ___mechLab);
                    __instance.OnAddItem(slot, false);
                    ___mechLab.ValidateLoadout(false);
                }
                catch (Exception e)
                {
                    Control.Logger.LogDebug("OnItemGrab.Postfix Error:", e);
                }
            }
        }
    }
}
