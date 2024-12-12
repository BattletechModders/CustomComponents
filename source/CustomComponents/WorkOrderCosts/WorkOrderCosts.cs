using System;
using System.Collections.Generic;
using BattleTech;
using fastJSON;

namespace CustomComponents;

[CustomComponent("WorkOrderCosts")]
public class WorkOrderCosts : SimpleCustomComponent, IOnLoaded
{
    public Costs Default { get; set; }
    public Costs Install { get; set; }
    public Costs Repair { get; set; }
    public Costs RepairDestroyed { get; set; }
    public Costs Remove { get; set; }
    public Costs RemoveDestroyed { get; set; }

    public class Costs
    {
        public string TechCost { get; set; }
        public string CBillCost { get; set; }

        [JsonIgnore]
        internal Func<MechDef, double> TechCostFunc;
        [JsonIgnore]
        internal Func<MechDef, double> CBillCostFunc;

        internal void Compile()
        {
            if (!string.IsNullOrEmpty(TechCost))
            {
                TechCostFunc = FormulaEvaluator.CompileMechDef(TechCost);
            }
            if (!string.IsNullOrEmpty(CBillCost))
            {
                CBillCostFunc = FormulaEvaluator.CompileMechDef(CBillCost);
            }
        }
    }

    public void OnLoaded()
    {
        Default?.Compile();
        Install?.Compile();
        Repair?.Compile();
        RepairDestroyed?.Compile();
        Remove?.Compile();
        RemoveDestroyed?.Compile();

        Install ??= Default;
        Repair ??= Default;
        RepairDestroyed ??= Default;
        Remove ??= Default;
        RemoveDestroyed ??= Default;
    }
}