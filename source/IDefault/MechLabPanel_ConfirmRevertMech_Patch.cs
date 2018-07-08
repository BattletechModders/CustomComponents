using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabPanel), "ConfirmRevertMech")]
    public static class MechLabPanel_ConfirmRevertMech_Patch
    {
        public static void Prefix(MechLabPanel __instance)
        {
            if(!__instance.IsSimGame)
                return;
            
            foreach (var item in __instance.baseWorkOrder.SubEntries.OfType<WorkOrderEntry_InstallComponent>().Where(i => i.MechComponentRef.Def is IDefault))
            {
                __instance.sim.WorkOrderComponents.Remove(item.MechComponentRef);
            }
        }
    }
}