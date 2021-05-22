using System.Linq;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents
{
    public class Change_WeaponAdjust : IChange_Adjust
    {
        public class usage_record
        {
            public HardpointExtentions.WeaponDefaultRecord weapon;
            public bool used_now;
            public bool used_after;

            public usage_record(HardpointExtentions.WeaponDefaultRecord weapon)
            {
                this.weapon = weapon;
                used_now = false;
                used_after = false;
            }
        }


        private ChassisLocations Location;

        public string ChangeID => "Weapon_" + Location;
        public bool Initial { get; set; }
        public void AdjustChange(InventoryOperationState state)
        {
            var defaults = state.Mech.GetWeaponDefaults();
            if (defaults == null)
                return;

            var usage = defaults
                .Where(i => i.Location == Location)
                .Select(i => new usage_record(i))
                .ToList();

            var loc_items = state.Inventory
                .Where(i => i.Location == Location)
                .Select(i => new
                {
                    item = i.Item,
                    location = i.Location,
                    usehp = i.Item.GetWeaponCategory(),
                    def = i.Item.IsDefault() && i.Item.IsModuleFixed(state.Mech),
                    cats = i.Item.GetComponents<Category>().Select(i => i.CategoryID).ToHashSet()
                })
                .Where(i => i.usehp != null)
                .ToList();

            for (int i = loc_items.Count - 1; i >= 0; i++)
            {
                var invItem = loc_items[i];
                var item = usage.FirstOrDefault(i => !i.used_now && i.weapon.Def.Description.Id == invItem.item.ComponentDefID);
                if (item != null)
                {
                    item.used_now = true;
                    loc_items.RemoveAt(i);
                }
            }

            foreach (var usageRecord in usage)
            {
                usageRecord.used_after = true;
                foreach (var locItem in loc_items.Where(i => !i.def))
                {
                    if (locItem.usehp.Name == usageRecord.weapon.WeaponCategory.Name &&
                        (!usageRecord.weapon.HaveCategories ||
                         usageRecord.weapon.Categories.Any(i => locItem.cats.Contains(i))))
                    {
                        usageRecord.used_after = false;
                        break;
                    }
                }

                if (usageRecord.used_after != usageRecord.used_now)
                    if (usageRecord.used_after)
                        state.AddChange(new Change_Add(usageRecord.weapon.Def.Description.Id,
                            usageRecord.weapon.Def.ComponentType, usageRecord.weapon.Location));
                    else
                        state.AddChange(new Change_Remove(usageRecord.weapon.Def.Description.Id, usageRecord.weapon.Location));
            }

        }

        public Change_WeaponAdjust(ChassisLocations location)
        {
            Location = location;
        }

    }
}