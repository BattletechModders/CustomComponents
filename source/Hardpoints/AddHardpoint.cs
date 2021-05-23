using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("AddHardpoint", true)]
    public class AddHardpoint : SimpleCustomComponent, IValueComponent<string>
    {
        public bool Valid => WeaponCategory != null && !WeaponCategory.Is_NotSet;

        public WeaponCategoryValue WeaponCategory { get; private set; }

        public void LoadValue(string value)
        {
            WeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(value);
 
            if (WeaponCategory == null)
                WeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
        }
    }

    [CustomComponent("ReplaceHardpoint", true)]
    public class ReplaceHardpoint : SimpleCustomComponent, IAfterLoad
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
    }
}