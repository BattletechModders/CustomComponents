using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

class AddCustomToWeaponPostProcessor : IPostProcessor
{
    public void PostProcess(object target, Dictionary<string, object> values)
    {
        if (target is not WeaponDef weapon)
        {
            return;
        }

        if (weapon.Is<UseHardpointCustom>())
        {
            return;
        }

        var hp = new UseHardpointCustom();
        weapon.AddComponent(hp);
        hp.LoadValue(weapon.WeaponCategoryValue.Name);
        hp.AdjustDescription();
    }
}