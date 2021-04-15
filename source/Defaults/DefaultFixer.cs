//undef CCDEBUG
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using HBS.Extensions;

namespace CustomComponents
{

    public class DefaultFixer
    {
        public class inv_change
        {
            private bool add;
            public bool IsAdd => add;
            public bool IsRemove => !add;

            public string Id { get; set; }
            public ChassisLocations Location { get; set; }

            private inv_change(bool add, string id, ChassisLocations location)
            {
                this.add = add;
                this.Location = location;
                this.Id = id;
            }

            public static inv_change Add(string id, ChassisLocations location)
            {
                return new inv_change(true, id, location);
            }

            public static inv_change Remove(string id, ChassisLocations location)
            {
                return new inv_change(false, id, location);
            }
        }


        private static DefaultFixer _instance;

        public static DefaultFixer Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new DefaultFixer();
                return _instance;
            }
        }

        public List<inv_change> GetMultiChange(MechDef mech, IEnumerable<InvItem> inventory)
        {
            var defaults = DefaultsDatabase.Instance[mech];
            if (defaults == null || defaults.Multi == null || defaults.Multi.Count == 0)
                return null;


            return null;
        }
    }
}