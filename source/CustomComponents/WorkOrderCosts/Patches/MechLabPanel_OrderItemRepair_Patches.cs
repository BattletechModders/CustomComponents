using BattleTech.UI;

namespace CustomComponents;

[HarmonyPatch(typeof(MechLabPanel), "OrderItemRepair")]
public static class MechLabPanel_OrderItemRepair_Patches
{
    public static void Prefix(bool __runOriginal, MechLabPanel __instance)
    {    
        if (!__runOriginal)
        {
            return;
        }

        WorkOrderCostsHandler.Shared.ActiveMechDef = __instance.Sim.GetMechByID(__instance.baseWorkOrder.MechID);
    }

    public static void Postfix(MechLabPanel __instance)
    {
        WorkOrderCostsHandler.Shared.ActiveMechDef = null;
    }
}