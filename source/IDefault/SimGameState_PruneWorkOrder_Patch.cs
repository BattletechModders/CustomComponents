using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "PruneWorkOrder")]
    public static class SimGameState_PruneWorkOrder_Patch
    {
        public static void Prefix(ref WorkOrderEntry_MechLab baseWorkOrder)
        {
            DefaultHelper.PrefixPrune(baseWorkOrder);

        }

        public static void Postfix(ref WorkOrderEntry_MechLab baseWorkOrder)
        {
            DefaultHelper.PostfixPrune(baseWorkOrder);

        }
    }
}