using BattleTech;

namespace CustomComponents;

internal class WorkOrderCostsHandler
{
    public static readonly WorkOrderCostsHandler Shared = new();

    public void ComponentInstallWorkOrder(MechDef mechDef, MechComponentRef mechComponent, ChassisLocations newLocation, WorkOrderEntry_InstallComponent result)
    {

        var workOrderCosts = mechComponent.Def.GetComponent<WorkOrderCosts>();
        if (workOrderCosts == null)
        {
            return;
        }

        if (newLocation == ChassisLocations.None) // remove
        {
            if (mechComponent.DamageLevel == ComponentDamageLevel.Destroyed)
            {
                ApplyCosts(result, workOrderCosts.RemoveDestroyed, mechDef);
            }
            else
            {
                ApplyCosts(result, workOrderCosts.Remove, mechDef);
            }
        }
        else // install
        {
            ApplyCosts(result, workOrderCosts.Install, mechDef);
        }
    }

    public void ComponentRepairWorkOrder(MechComponentRef mechComponent, bool isOnMech, WorkOrderEntry_RepairComponent result)
    {
        var workOrderCosts = mechComponent.Def.GetComponent<WorkOrderCosts>();
        if (workOrderCosts == null)
        {
            return;
        }

        if (mechComponent.DamageLevel == ComponentDamageLevel.Destroyed)
        {
            ApplyCosts(result, workOrderCosts.RepairDestroyed, null);
        }
        else
        {
            ApplyCosts(result, workOrderCosts.Repair, null);
        }
    }

    private void ApplyCosts(WorkOrderEntry_MechLab workOrder, WorkOrderCosts.Costs costs, MechDef mechDef)
    {
        if (costs == null)
        {
            return;
        }

        if (costs.CBillCostFunc is not null)
        {
            workOrder.CBillCost = (int)costs.CBillCostFunc(mechDef);
        }
        if (costs.TechCostFunc is not null)
        {
            workOrder.Cost = (int)costs.TechCostFunc(mechDef);
        }
    }
}