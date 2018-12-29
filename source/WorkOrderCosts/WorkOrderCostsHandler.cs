using System;
using System.Collections.Generic;
using System.Globalization;
using BattleTech;

namespace CustomComponents
{
    internal class WorkOrderCostsHandler
    {
        public static readonly WorkOrderCostsHandler Shared = new WorkOrderCostsHandler();

        public void ComponentInstallWorkOrder(MechDef mechDef, MechComponentRef mechComponent, ChassisLocations newLocation, WorkOrderEntry_InstallComponent result)
        {

            var workOrderCosts = mechComponent.Def.GetComponent<WorkOrderCosts>();
            if (workOrderCosts == null)
            {
                return;
            }

            var variables = mechDef == null ? null : TemplateVariables(mechDef);

            if (newLocation == ChassisLocations.None) // remove
            {
                if (mechComponent.DamageLevel == ComponentDamageLevel.Destroyed)
                {
                    ApplyCosts(result, workOrderCosts.RemoveDestroyed, variables);
                }
                else
                {
                    ApplyCosts(result, workOrderCosts.Remove, variables);
                }
            }
            else // install
            {
                ApplyCosts(result, workOrderCosts.Install, variables);
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
                ApplyCosts(result, workOrderCosts.RepairDestroyed);
            }
            else
            {
                ApplyCosts(result, workOrderCosts.Repair);
            }
        }
        
        private Dictionary<string, string> TemplateVariables(MechDef mechDef)
        {
            if (mechDef == null)
            {
                return null;
            }

            var variables = new Dictionary<string, string>
            {
                ["Chassis.Tonnage"] = mechDef.Chassis.Tonnage.ToString(CultureInfo.InvariantCulture)
            };

            return variables;
        }

        private void ApplyCosts(WorkOrderEntry_MechLab workOrder, WorkOrderCosts.Costs costs, Dictionary<string, string> variables = null)
        {
            if (costs == null)
            {
                return;
            }

            var adapter = new WorkOrderEntry_MechLabAdapter(workOrder);
            if (!string.IsNullOrEmpty(costs.CBillCost))
            {
                adapter.CBillCost = Convert.ToInt32(FormulaEvaluator.Shared.Evaluate(costs.CBillCost, variables));
            }
            if (!string.IsNullOrEmpty(costs.TechCost))
            {
                adapter.Cost = Convert.ToInt32(FormulaEvaluator.Shared.Evaluate(costs.TechCost, variables));
            }
        }
    }
}