using BattleTech;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public class DefaultsInfo : IDefault
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;

        public override string ToString()
        {
            return "DefaultsInfo: " + DefID;
        }
    }

    public class UTDefaultRecord : IDefault
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;

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