using BattleTech;
using System.Collections.Generic;
using System.Linq;
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
                if (mechdef == null || itemdef == null)
                    return ChassisLocations.None;

                if (!Locations.TryGetValue(mechdef.ChassisID, out var mech_locations))
                {
                    mech_locations = new Dictionary<string, ChassisLocations>();
                    Locations[mechdef.ChassisID] = mech_locations;
                }

                if (!mech_locations.TryGetValue(itemdef.Description.Id, out var location))
                {
                    
                    if (itemdef.Is<EquipLocations>(out var el))
                    {
                        var info = GetAllowedLocations(mechdef, el.Tag);
                        if (info == null)
                        {
                            if (Tags.TryGetValue(el.Tag, out var record))
                            {

                                var ut = mechdef.GetUnitTypes();
                                if (record.UnitTypes == null || record.UnitTypes.Length == 0 || ut == null)
                                    location = itemdef.AllowedLocations & record.Default;
                                else
                                {
                                    var lr = record.UnitTypes.FirstOrDefault(i => ut.Contains(i.UnitType));
                                    location = itemdef.AllowedLocations & (lr?.Location ?? record.Default);
                                }
                            }
                            else
                                location = itemdef.AllowedLocations;
                        }
                        else
                            location = info.Locations;

                    }
                    else
                    {
                        location = itemdef.AllowedLocations;
                    }
                    mech_locations[itemdef.Description.Id] = location;
                }

                return location;
            }
        }

        private IChassisAllowedLocations GetAllowedLocations(MechDef mechdef, string tag)
        {
            return mechdef.Chassis.GetComponents<IChassisAllowedLocations>().FirstOrDefault(i => i.Tag == tag);
        }

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var tag in SettingsResourcesTools.Enumerate<EquipLocationTag>("CCCEquipLocationTag", customResources))
            {
                Tags[tag.Tag] = tag;
                Logging.Info?.Log($"LocationTag {tag.Tag} registered");
                if (Control.Settings.DEBUG_ShowLoadedAlLocations)
                    Logging.Info?.Log(tag.ToString());
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
