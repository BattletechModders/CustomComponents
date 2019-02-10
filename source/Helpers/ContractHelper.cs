using System;
using System.Collections.Generic;
using System.Reflection;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    public class ContractHelper
    {
        private readonly Traverse main;

        private readonly Contract contract;
        private readonly Traverse<List<SalvageDef>> salvaged_chassis;
        private readonly Traverse<List<MechDef>> lost_mechs;
        private readonly Traverse<List<SalvageDef>> salvage_results;
        private readonly Traverse<int> salvage_count;
        private readonly Traverse<int> prio_salvage_count;

        private readonly MethodInfo create_mech_part;
        private readonly MethodInfo create_component;
        private readonly MethodInfo filter_salvage;

        public List<SalvageDef> SalvagedChassis
        {
            get => salvaged_chassis.Value;
            set => salvaged_chassis.Value = value;
        }

        public List<SalvageDef> SalvageResults
        {
            get => salvage_results.Value;
            set => salvage_results.Value = value;
        }

        public List<MechDef> LostMechs
        {
            get => lost_mechs.Value;
            set => lost_mechs.Value = value;
        }

        public int FinalSalvageCount { get => salvage_count.Value; set => salvage_count.Value = value; }
        public int FinalPrioritySalvageCount { get => prio_salvage_count.Value; set => prio_salvage_count.Value = value; }


        public ContractHelper(Contract contract)
        {
            main = new Traverse(contract);
            this.contract = contract;

            salvage_results = main.Property<List<SalvageDef>>("SalvageResults");
            salvaged_chassis = main.Property<List<SalvageDef>>("SalvagedChassis");
            lost_mechs = main.Property<List<MechDef>>("LostMechs");
            salvage_count = main.Property<int>("FinalSalvageCount");
            prio_salvage_count = main.Property<int>("FinalPrioritySalvageCount");

            create_mech_part = contract.GetType().GetMethod("CreateAndAddMechPart", BindingFlags.NonPublic | BindingFlags.Instance);
            create_component = contract.GetType().GetMethod("AddMechComponentToSalvage", BindingFlags.NonPublic | BindingFlags.Instance); 
            filter_salvage = contract.GetType().GetMethod("FilterPotentialSalvage", BindingFlags.NonPublic | BindingFlags.Instance);

            if (create_mech_part == null)
                Control.LogError("create_mech_part = null");

            if (create_component == null)
                Control.LogError("create_component = null");

            if (filter_salvage == null)
                Control.LogError("filter_salvage = null");

        }

        public void CreateAndAddMechPart(SimGameConstants sc, MechDef m, int count, List<SalvageDef> salvageList)
        {
            create_mech_part.Invoke(contract, new Object[] { sc, m, count, salvageList });
        }

        public void AddMechComponentToSalvage(List<SalvageDef> salvageList, MechComponentDef def, ComponentDamageLevel damageLevel, bool breakComponents, SimGameConstants sc, NetworkRandom rand, bool chanceForUpgrade = true)
        {
            create_component.Invoke(contract, new Object[] { salvageList, def, damageLevel, breakComponents, sc, rand, chanceForUpgrade });
        }

        public void FilterPotentialSalvage(List<SalvageDef> salvageDefs)
        {
            filter_salvage.Invoke(contract, new Object[] { salvageDefs });
        }
    }
}