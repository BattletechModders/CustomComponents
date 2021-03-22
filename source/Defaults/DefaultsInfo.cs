using BattleTech;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public class DefaultsInfo : IDefault
    {
        public string Tag { get; set; }

        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;
        public bool AddIfNotPresent { get; set; } = true;

        public MechComponentRef GetReplace(MechDef mechDef, SimGameState state)
        {
            var res = DefaultHelper.CreateRef(DefID, Type, mechDef.DataManager, state);
            res.SetData(Location, -1, ComponentDamageLevel.Functional, true);
            return res;
        }

        public virtual bool AddItems(MechDef mechDef, SimGameState state)
        {
            if (AddIfNotPresent)
            {
                DefaultHelper.AddInventory(DefID, mechDef, Location, Type, state);
                return true;
            }
            return false;
        }

        public bool NeedReplaceExistDefault(MechDef mechDef, MechComponentRef item)
        {
            return item.ComponentDefID != DefID;
        }

        public override string ToString()
        {
            return "DefaultsInfo: " + DefID;
        }
    }

    public class UTDefaultRecord
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;
        public bool AddIfNotPresent { get; set; } = true;

        [JsonIgnore] public bool Ready { get; private set; } = false;
        public bool Invalid => Ready && _def == null;

        private MechComponentDef _def;
        private Category _cat;

        [JsonIgnore]
        public MechComponentDef Def
        {
            get
            {
                if (!Ready)
                    Init();
                return _def;
            }
        }

        [JsonIgnore]
        public Category Category
        {
            get
            {
                if(!Ready)
                    Init();
                return _cat;
            }
        }

        private void Init()
        {
            Ready = true;
            _def = DefaultHelper.GetComponentDef(DefID, Type);
            if (_def == null)
                return;
            if (!_def.IsCategory(CategoryID, out _cat))
            {
                _def = null;
                return;
            }
        }

    }

    [SerializeField]
    public class UTDefaultInfo
    {
        public string UnitType { get; set; }
        public UTDefaultRecord[] Records;

    }
}