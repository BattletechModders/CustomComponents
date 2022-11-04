using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Localize;

namespace CustomComponents
{
    internal static class WeaponsCountFix
    {
        internal static void CheckWeapons(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            if (!CheckWeaponsFielded(mechDef))
            {
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(Control.Settings.Message.WrongWeaponCount, Control.Settings.MaxWeaponCount));
            }
        }

        internal static bool CheckWeaponsFielded(MechDef mechDef)
        {
            var count = mechDef.Inventory.Count(i => i.ComponentDefType == ComponentType.Weapon);
            return count <= Control.Settings.MaxWeaponCount;
        }
    }
}
