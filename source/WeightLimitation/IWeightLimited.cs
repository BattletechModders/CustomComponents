using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component limited to mech tonnage
    /// </summary>
    [CustomComponent("WeightLimit")]
    public class WeightLimited : SimpleCustomComponent, IMechLabFilter
    {
        /// <summary>
        /// minimum allowed tonnage
        /// </summary>
        public int MinTonnage { get; set; }
        /// <summary>
        /// maximum allowed tonnage
        /// </summary>
        public int MaxTonnage { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return MinTonnage >= tonnage && MaxTonnage <= tonnage;
        }
    }

    [CustomComponent("WeightAllowed")]
    public class WeightAllowed : SimpleCustomComponent, IMechLabFilter
    {
        public int AllowedTonnage { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return AllowedTonnage == tonnage;
        }
    }
}
