using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.ExtendedDetails;

namespace CustomComponents
{
    [CustomComponent("AddHardpoint")]
    public class AddHardpoint : SimpleCustomComponent, IValueComponent<string>, IPreValidateDrop, IAdjustDescription, IValid
    {
        public bool Valid => WeaponCategory != null && !WeaponCategory.Is_NotSet;

        public WeaponCategoryValue WeaponCategory { get; private set; }
        public HardpointInfo HPinfo { get; private set; }

        public void LoadValue(string value)
        {
            WeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(value);
 
            if (WeaponCategory == null)
                WeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();

            if (!WeaponCategory.Is_NotSet)
                HPinfo = HardpointController.Instance[WeaponCategory];
        }

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            if (!Valid)
                return string.Empty;

            var hardpoints = MechLabHelper.CurrentMechLab.ActiveMech.GetAllHardpoints(location);

            int n = 0;
            foreach (var  hardpoint in hardpoints)
            {
                if (hardpoint.hpInfo.Visible)
                {
                    n += 1;
                    if (hardpoint.hpInfo.WeaponCategory.ID == WeaponCategory.ID)
                        return string.Empty;
                }
            }

            if (n >= 4)
            {
                return Control.Settings.Message.Hardpoints_TooManyHardpoints;
            }

            return string.Empty;
        }

        public void AdjustDescription()
        {
            if (!Valid)
                return;

            var hpinfo = HardpointController.Instance[WeaponCategory];
            if (hpinfo == null || !hpinfo.Visible)
                return;

            var ed = ExtendedDetails.ExtendedDetails.GetOrCreate(Def);
            var detail =
                ed.GetDetails().FirstOrDefault(i => i.Identifier == "AddHardpoints") as
                    ExtendedDetails.ExtendedDetailList ??
                new ExtendedDetail()
                {
                    Index = Control.Settings.HardpointAddIndex,
                    Identifier = "AddHardpoints",
                    Text = "\n<b>Add Hardpoint: <color=" +Control.Settings.HardpointDescriptionColor + ">"
                           + WeaponCategory.FriendlyName
                           + "</color></b>\n"
                };

            ed.AddDetail(detail);
        }
    }

    [CustomComponent("ReplaceHardpoint")]
    public class ReplaceHardpoint : SimpleCustomComponent, IAfterLoad, IPreValidateDrop, IAdjustDescription, IValid
    {
        private string UseHardpoint;
        private string AddHardpoint;

        public bool Valid => AddWeaponCategory != null && !AddWeaponCategory.Is_NotSet 
            && UseWeaponCategory != null && !UseWeaponCategory.Is_NotSet;

        public WeaponCategoryValue AddWeaponCategory { get; private set; }
        public WeaponCategoryValue UseWeaponCategory { get; private set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            AddWeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(AddHardpoint);

            if (AddWeaponCategory == null)
                AddWeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();

            UseWeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(UseHardpoint);

            if (UseWeaponCategory == null)
                UseWeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
        }

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            return string.Empty;
        }

        public void AdjustDescription()
        {
            if (!Valid)
                return;

            var ahpinfo = HardpointController.Instance[AddWeaponCategory];
            var rhpinfo = HardpointController.Instance[UseWeaponCategory];

            var ed = ExtendedDetails.ExtendedDetails.GetOrCreate(Def);
            var detail =
                ed.GetDetails().FirstOrDefault(i => i.Identifier == "AddHardpoints") as
                    ExtendedDetails.ExtendedDetailList ??
                new ExtendedDetail()
                {
                    Index = Control.Settings.HardpointAddIndex,
                    Identifier = "AddHardpoints",
                    Text = "\n<b>Replace <color=" + Control.Settings.HardpointDescriptionColor + ">"
                           + UseWeaponCategory.FriendlyName
                           + "</color></b> Hardpoint with <color=" + Control.Settings.HardpointDescriptionColor + ">"
                           + AddWeaponCategory.FriendlyName
                           + "</color></b>\n"

                };

            ed.AddDetail(detail);
        }
    }
}