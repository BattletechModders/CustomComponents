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
        private Dictionary<string, EquipLocationTag> Tags = new Dictionary<string, EquipLocationTag>();
        public ChassisLocations this[MechDef mechdef, MechComponentDef itemdef]
        {
            get
            {
                ChassisLocations get_location(ChassisLocations Default, EquipLocationTag.record[] records)
                {
                    var ut = UnitTypeDatabase.Instance[mechdef];
                    if (records == null || records.Length == 0 || ut != null || ut.Length == 0)
                        return itemdef.AllowedLocations & Default;
                    var lr = records.FirstOrDefault(i => ut.Contains(i.UnitType));
                    return itemdef.AllowedLocations & (lr?.Location ?? Default);
                }

                if (mechdef == null || itemdef == null)
                    return ChassisLocations.None;

                if (!Locations.TryGetValue(mechdef.ChassisID, out var ld))
                {
                    ld = new Dictionary<string, ChassisLocations>();
                    Locations[mechdef.ChassisID] = ld;
                }

                if (!ld.TryGetValue(itemdef.Description.Id, out var location))
                {
                    if (itemdef.Is<EquipLocationsUT>(out var el))
                    {
                        location = get_location(el.Default, el.UnitTypes);
                    }
                    else if (itemdef.Is<EquipLocationsTAG>(out var elt))
                    {
                        if (Tags.TryGetValue(elt.Tag, out var tag))
                        {
                            location = get_location(tag.Default, tag.UnitTypes);
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
            foreach (var tag in SettingsResourcesTools.Enumerate<EquipLocationTag>("CCCEquipLocationTag", customResources))
            {
                Tags[tag.Tag] = tag;
            }
        }
    }
}
