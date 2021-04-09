using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomComponents
{
    public class EquipLocationController
    {
        private static EquipLocationController _instance;

        public static EquipLocationController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EquipLocationController();

                return _instance;
            }
        }

        private Dictionary<string, Dictionary<string, ChassisLocations>> Locations = new Dictionary<string, Dictionary<string, ChassisLocations>>();
        private Dictionary<string, LocationRedefineTag> Tags = new Dictionary<string, LocationRedefineTag>();
        public ChassisLocations this[MechDef mechdef, MechComponentDef itemdef]
        {
            get 
            {
                if (mechdef == null || itemdef == null)
                    return ChassisLocations.None;

                if(!Locations.TryGetValue(mechdef.ChassisID, out var ld))
                {
                    ld = new Dictionary<string, ChassisLocations>();
                    Locations[mechdef.ChassisID] = ld;  
                }

                if (!ld.TryGetValue(itemdef.Description.Id, out var location))
                {
                    if (itemdef.Is<EquipLocations>(out var el))
                    {
                        location = itemdef.AllowedLocations & el.Locations;
                    }
                    else if (itemdef.Is<EquipLocationsTag>(out var elt))
                    {
                        if (Tags.TryGetValue(elt.Tag, out var tag))
                        {
                            var ut = UnitTypeDatabase.Instance[mechdef];
                            if (ut != null || ut.Length == 0)
                                location = itemdef.AllowedLocations & tag.Default;
                            else
                            {
                                var lr = tag.UnitTypes.FirstOrDefault(i => ut.Contains(i.UnitType));
                                location = itemdef.AllowedLocations & ( lr?.Location ?? tag.Default);
                            }
                        }
                        else
                        {
                            Control.LogError($"{itemdef.Description.Id} have unknow LocationOverridTag '{elt.Tag}'");
                            location = itemdef.AllowedLocations;
                        }
                    }
                    else
                    {
                        location = itemdef.AllowedLocations;
                    }
                    ld[itemdef.Description.Id] = location;
                }

                return location;
            }
        }

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var tag in SettingsResourcesTools.Enumerate<LocationRedefineTag>("CCCEquipLocationTag", customResources))
            {
                Tags[tag.Tag] = tag;
            }
        }
    }
}
