using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech.UI;
using Localize;

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

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            var allowed = item.ComponentRef.Is<IAllowedLocations>(out var al)
                ? al.GetLocationsFor(MechLabHelper.CurrentMechLab.ActiveMech)
                : item.ComponentRef.Def.AllowedLocations;


            if ((location & allowed) <= ChassisLocations.None)
                return new Text(Control.Settings.Message.Base_AddWrongLocation,
                    item.ComponentRef.Def.Description.Name, location, 
                    item.ComponentRef.Def.Description.UIName).ToString();

            return string.Empty;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationlevel, MechDef mechdef)
        {
            foreach (var item in mechdef.Inventory)
            {
                var location = item.Is<IAllowedLocations>(out var al)
                    ? al.GetLocationsFor(mechdef)
                    : item.Def.AllowedLocations;

                if ((location & item.MountedLocation) <= ChassisLocations.None)
                    errors[MechValidationType.InvalidInventorySlots].Add(
                        new Text(Control.Settings.Message.Base_ValidateWrongLocation, item.Def.Description.Name)
                        );
            }
        }

        public bool CanBeFielded(MechDef mechdef)
        {
            foreach (var item in mechdef.Inventory)
            {
                var location = item.Is<IAllowedLocations>(out var al)
                    ? al.GetLocationsFor(mechdef)
                    : item.Def.AllowedLocations;

                if ((location & item.MountedLocation) <= ChassisLocations.None)
                    return false;
            }

            return true;
        }
    }
}
