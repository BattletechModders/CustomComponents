using System;
using System.CodeDom;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("UseHardpoint")]
    public class UseHardpointCustom : SimpleCustomComponent, IValueComponent, IOnAdd, IOnRemove
    {
        public WeaponCategoryValue WeaponCategory { get; private set; } = WeaponCategoryEnumeration.GetNotSetValue();

        public void LoadValue(object value)
        {
            var cat = WeaponCategoryEnumeration.GetWeaponCategoryByName(value.ToString());
            WeaponCategory = cat ?? WeaponCategoryEnumeration.GetNotSetValue();

        }

        internal void SetValue(WeaponCategoryValue value)
        {
            WeaponCategory = value ?? WeaponCategoryEnumeration.GetNotSetValue();
        }

        public void OnAdd(ChassisLocations location, InventoryOperationState state)
        {
            if(!Def.IsDefault() && state.Mech.HasWeaponDefaults())
                state.AddChange(new Change_WeaponAdjust());
        }

        public void OnRemove(ChassisLocations location, InventoryOperationState state)
        {
            if (!Def.IsDefault() && state.Mech.HasWeaponDefaults())
                state.AddChange(new Change_WeaponAdjust());
        }
    }
}