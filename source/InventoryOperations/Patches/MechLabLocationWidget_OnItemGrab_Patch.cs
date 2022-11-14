using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
using Harmony;
using Localize;

namespace CustomComponents.Patches
{
    /// <summary>
    /// ItemGrab path for IDefaultReplace
    /// </summary>
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    internal static class MechLabLocationWidget_OnItemGrab_Patch_IDefaultReplace
    {
        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab, ref MechComponentRef __state)
        {
            try
            {
                Log.ComponentInstall.Trace?.Log($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

                foreach (var grab_handler in item.ComponentRef.Def.GetComponents<IOnItemGrab>())
                {
                    if (item.ComponentRef.Flags<CCFlags>().NoRemove)
                    {
                        __result = false;
                        return false;
                    }

                    if (!grab_handler.OnItemGrab(item, ___mechLab, out var error))
                    {
                        if (!string.IsNullOrEmpty(error))
                            ___mechLab.ShowDropErrorMessage(new Text(error));
                        __result = false;
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
            return true;
        }

        public static void Postfix(IMechLabDraggableItem item, ref bool __result, MechComponentRef __state,
            MechLabPanel ___mechLab, MechLabLocationWidget __instance)
        {
            try
            {
                if (!__result)
                    return;


                var changes = new Queue<IChange>();
                changes.Enqueue(new Change_Remove(item.ComponentRef, __instance.loadout.Location, true));
                var state = new InventoryOperationState(changes, MechLabHelper.CurrentMechLab.ActiveMech);
                state.DoChanges();
                state.ApplyMechlab();

                ___mechLab.ValidateLoadout(false);

            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
        }
    }
}