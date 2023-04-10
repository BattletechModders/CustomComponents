using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;

namespace CustomComponents.Patches;

/// <summary>
/// ItemGrab path for IDefaultReplace
/// </summary>
[HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.OnItemGrab))]
internal static class MechLabLocationWidget_OnItemGrab_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab, ref MechComponentRef __state)
    {
        if (!__runOriginal)
        {
            return;
        }

        Log.ComponentInstall.Trace?.Log($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

        foreach (var grab_handler in item.ComponentRef.Def.GetComponents<IOnItemGrab>())
        {
            if (item.ComponentRef.Def.CCFlags().NoRemove)
            {
                __result = false;
                __runOriginal = false;
                return;
            }

            if (!grab_handler.OnItemGrab(item, ___mechLab, out var error))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    ___mechLab.ShowDropErrorMessage(new(error));
                }

                __result = false;
                __runOriginal = false;
                return;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(IMechLabDraggableItem item, ref bool __result, MechComponentRef __state,
        MechLabPanel ___mechLab, MechLabLocationWidget __instance)
    {
        if (!__result)
        {
            return;
        }

        var changes = new Queue<IChange>();
        changes.Enqueue(new Change_Remove(item.ComponentRef, __instance.loadout.Location, true));
        var state = new InventoryOperationState(changes, MechLabHelper.CurrentMechLab.ActiveMech);
        state.DoChanges();
        state.ApplyMechlab();

        ___mechLab.ValidateLoadout(false);
    }
}