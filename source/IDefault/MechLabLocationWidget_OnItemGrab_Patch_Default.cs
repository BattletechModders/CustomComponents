using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    public static class MechLabLocationWidget_OnItemGrab_Patch
    {
        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab)
        {
            //Control.Logger.LogDebug($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            if (item.ComponentRef.Def is ICannotRemove)
            {
                ___mechLab.ShowDropErrorMessage("Cannot remove vital component");
                __result = false;
                return false;
            }

            return true;
        }
    }
}