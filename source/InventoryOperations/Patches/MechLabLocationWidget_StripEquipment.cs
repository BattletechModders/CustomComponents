using System.Collections.Generic;
using BattleTech.UI;
using CustomComponents.Changes;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "StripEquipment")]
    internal static class MechLabLocationWidget_StripEquipment_Patch
    {
        [HarmonyPrefix]
        public static bool StripLocation(MechLabLocationWidget __instance)
        {
            Control.LogDebug(DType.InventoryOperaions, $"StripEquipment in {__instance.loadout.Location}");
            var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);

            var changes = new Queue<IChange>();

            foreach (var item in lhelper.LocalInventory)
                if (!item.ComponentRef.IsFixed)
                {
                    changes.Enqueue(new Change_Remove(item.ComponentRef.ComponentDefID, __instance.loadout.Location));
                    Control.LogDebug(DType.InventoryOperaions, $"- remove {item.ComponentRef.ComponentDefID}");
                }
                else
                {
                    Control.LogDebug(DType.InventoryOperaions, $"- fixed {item.ComponentRef.ComponentDefID}");
                }

            var state = new InventoryOperationState(changes, MechLabHelper.CurrentMechLab.ActiveMech);
            state.DoChanges();
            state.ApplyMechlab();

            return false;
        }
    }
}