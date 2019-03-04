using BattleTech;

namespace CustomComponents
{
    [CustomComponent("ChassisDefaults", true)]
    public class ChassisDefaults : SimpleCustomChassis, IDefault, IReplaceIdentifier
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;

        public string ReplaceID => DefaultFixer.Shared.GetID(this);

        public MechComponentRef GetReplace(MechDef mechDef, SimGameState state)
        {
            var res = DefaultHelper.CreateRef(DefID, Type, mechDef.DataManager, state);
            res.SetData(Location, -1, ComponentDamageLevel.Functional, true);
            return res;
        }

        public bool AddItems(MechDef mechDef, SimGameState state)
        {
            DefaultHelper.AddInventory(DefID, mechDef, Location, Type, state);
            return true;
        }

        public bool NeedReplaceExistDefault(MechDef mechDef, MechComponentRef item)
        {
            return item.ComponentDefID != DefID; 
        }

        public override string ToString()
        {
            return $"ChassisDefaults: {DefID}";
        }
    }
}