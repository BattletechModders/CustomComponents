using BattleTech.UI;

namespace CustomComponents;

[HarmonyPatch(typeof(MechBayPanel), "OnRepairMech")]
public static class MechBayPanel_OnRepairMech_Patches
{
    public static void Prefix(ref bool __runOriginal, MechBayPanel __instance, MechBayMechUnitElement mechElement)
    {
        if (!__runOriginal)
        {
            return;
        }

        WorkOrderCostsHandler.Shared.ActiveMechDef = mechElement.MechDef;
    }

    public static void Postfix(MechBayPanel __instance)
    {
        WorkOrderCostsHandler.Shared.ActiveMechDef = null;
    }
}

[HarmonyPatch(typeof(MechBayPanel), "OrderItemRepair")]
public static class MechBayPanel_OrderItemRepair_Patches
{
    public static void Prefix(ref bool __runOriginal, MechBayPanel __instance, IMechLabDraggableItem item)
    {
        if (!__runOriginal)
        {
            return;
        }
        
        WorkOrderCostsHandler.Shared.ActiveMechDef = item.MechDef;
    }

    public static void Postfix(MechBayPanel __instance)
    {
        WorkOrderCostsHandler.Shared.ActiveMechDef = null;
    }
}