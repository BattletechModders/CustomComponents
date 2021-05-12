using System.Collections.Generic;
using BattleTech.UI;
using CustomComponents.Changes;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "StripEquipment")]
    internal static class MechLabLocationWidget_StripEquipment_Patch
    {
        public static bool StripLocation(MechLabLocationWidget __instance)
        {
            var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);

            var changes = new Queue<IChange>();

            foreach (var item in lhelper.LocalInventory)
                if (!item.ComponentRef.IsFixed)
                    changes.Add(new Change_Remove(item.ComponentRef.ComponentDefID, __instance.loadout.Location));

            var state = new InventoryOperationState(changes, MechLabHelper.CurrentMechLab.ActiveMech);
            state.ApplyMechlab();

            return false;
        }
    }
}