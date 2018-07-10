using System;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using fastJSON;

namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    [CustomComponent("Category")]
    public class Category : SimpleCustomComponent, IAfterLoad, IOnInstalled, IReplaceValidateDrop, IPostValidateDrop
    {
        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        string Tag;

        public string GetTag()
        {
            if (string.IsNullOrEmpty(Tag))
                return Def.Description.Id;
            else
                return Tag;
        }

        [JsonIgnore]
        public CategoryDescriptor CategoryDescriptor { get; set; }

        public void OnLoaded()
        {
            CategoryDescriptor = Control.GetCategory(CategoryID);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {

            Control.Logger.LogDebug($"- Category");

            if (CategoryDescriptor.AutoReplace || CategoryDescriptor.MaxEquiped < 0 && CategoryDescriptor.MaxEquipedPerLocation < 0)
            {
                var c1 = CategoryDescriptor;
                Control.Logger.LogDebug($"-- {c1.DisplayName} r:{c1.AutoReplace}  max:{c1.MaxEquiped} mpr:{c1.MaxEquipedPerLocation} = not requre replace");
            }

            if (order.DesiredLocation == ChassisLocations.None)
            {
                Control.Logger.LogDebug("-- removing, no additional actions");
                return;
            }


            int n1 = mech.Inventory.Count(i => i.Is<Category>(out var cat) && CategoryID == cat.CategoryID);
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.Is<Category>(out var cat)
                   && CategoryID == cat.CategoryID);

            Control.Logger.LogDebug($"-- total {n1}/{CategoryDescriptor.MaxEquiped}  location: {n2}/{CategoryDescriptor.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => i.MountedLocation == order.DesiredLocation && i.Is<Category>(out var cat)
                   && CategoryID == cat.CategoryID && i.Is<Flags>(out var f) && f.Default);

            Control.Logger.LogDebug($"-- possible replace: {(replace == null ? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (CategoryDescriptor.MaxEquiped > 0 && n1 > CategoryDescriptor.MaxEquiped) ||
                (CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > CategoryDescriptor.MaxEquipedPerLocation);

            Control.Logger.LogDebug($"-- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveDefault(replace.ComponentDefID, mech, order.DesiredLocation, replace.ComponentDefType);

        }

        public IValidateDropResult ReplaceValidateDrop(MechLabItemSlotElement element, LocationHelper location,
            IValidateDropResult last_result)
        {
            //if replace already done
            if (last_result is ValidateDropChange change && change.Changes.Any(i => i is SlotChange))
                return last_result;

            //if cannot do replace
            if (!CategoryDescriptor.AutoReplace ||
                (CategoryDescriptor.MaxEquiped <= 0 && CategoryDescriptor.MaxEquipedPerLocation <= 0))
                return last_result;

            if (CategoryDescriptor.MaxEquiped > 0)
            {
                var n = location.mechLab.activeMechDef.Inventory
                    .Select(i => i.Def.GetComponent<Category>())
                    .Count(i => i != null && i.CategoryID == CategoryID);

                if (n > CategoryDescriptor.MaxEquiped)
                    return new ValidateDropError(string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName, n));

                if (n == CategoryDescriptor.MaxEquiped)
                {
                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.Is<Category>(out var c) && c.CategoryID == CategoryID);
                    if (replace == null)
                        if (CategoryDescriptor.MaxEquiped > 1)
                            return new ValidateDropError(string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName, n));
                        else
                            return new ValidateDropError(string.Format(CategoryDescriptor.AddAlreadyEquiped, CategoryDescriptor.displayName));

                    return ValidateDropChange.AddOrCreate(last_result,
                        new AddChange(location.widget.loadout.Location, replace));
                }
            }

            if (CategoryDescriptor.MaxEquipedPerLocation > 0)
            {
                var n = location.LocalInventory
                    .Select(i => i.ComponentRef.Def.GetComponent<Category>())
                    .Count(i => i != null && i.CategoryID == CategoryID);

                if (n > CategoryDescriptor.MaxEquipedPerLocation)
                    return new ValidateDropError(string.Format(CategoryDescriptor.AddMaximumLocationReached, CategoryDescriptor.displayName, n, location.LocationName));

                if (n == CategoryDescriptor.MaxEquipedPerLocation)
                {
                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.Is<Category>(out var c) && c.CategoryID == CategoryID);

                    if (replace == null)
                        if (CategoryDescriptor.MaxEquipedPerLocation > 1)
                            return new ValidateDropError(string.Format(CategoryDescriptor.AddMaximumLocationReached, CategoryDescriptor.displayName, n, location.LocationName));
                        else
                            return new ValidateDropError(string.Format(CategoryDescriptor.AddAlreadyEquipedLocation, CategoryDescriptor.displayName, location.LocationName));

                    return ValidateDropChange.AddOrCreate(last_result,
                        new AddChange(location.widget.loadout.Location, replace));
                }

            }

            return last_result;
        }

        public IValidateDropResult PostValidateDrop(MechLabItemSlotElement element, LocationHelper location,
            IValidateDropResult last_result)
        {
            var c = CategoryDescriptor;

            var changes = last_result as ValidateDropChange;

            if (!c.AllowMixTags)
            {
                var tag = GetTag();

                foreach (var mref in location.mechLab.activeMechDef.Inventory.Where(i => i.Def.Is<Category>(out var c1) && c1.CategoryID == CategoryID && c1.GetTag() != GetTag()))
                {
                    if (changes != null && changes.Changes.All(i => !(i is RemoveChange change) || change.item.ComponentRef != mref))
                        return new ValidateDropError(string.Format(c.AddMixed, c.DisplayName));
                }
            }

            if (c.MaxEquiped > 0)
            {
                var n = location.mechLab.activeMechDef.Inventory
                    .Select(i => i.Def.GetComponent<Category>())
                    .Count(i => i != null && i.CategoryID == CategoryID) + 1;

                if (changes != null)
                    foreach (var change in changes.Changes.OfType<SlotChange>().Where(i => i.item.ComponentRef.Def.Is<Category>(out var c1) && c1.CategoryID == CategoryID))
                    {
                        if (change is AddChange)
                            n += 1;
                        else if (change is RemoveChange)
                            n -= 1;
                    }

                if (n > c.MaxEquiped)
                    if (CategoryDescriptor.MaxEquiped > 1)
                        return new ValidateDropError(string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName, n));
                    else
                        return new ValidateDropError(string.Format(CategoryDescriptor.AddAlreadyEquiped, CategoryDescriptor.displayName));
            }

            if (c.MaxEquipedPerLocation > 0)
            {
                var items_by_location = location.mechLab.activeMechInventory
                    .Where(i => i.Is<Category>(out var c1) && c1.CategoryID == CategoryID)
                    .Select(i => new {c = 1, location = i.MountedLocation});

                if (changes != null)
                    items_by_location.Union(changes.Changes.OfType<SlotChange>()
                        .Where(i => i.item.ComponentRef.Is<Category>(out var c1) && c1.CategoryID == CategoryID)
                        .Select(i => new { c = (i is AddChange ? 1 : -1), location = i.location }));

                if(items_by_location.GroupBy(i => i.location).Any(i => i.Sum(a => a.c) > c.MaxEquipedPerLocation))
                    if (c.MaxEquipedPerLocation > 1)
                        return new ValidateDropError(string.Format(c.AddMaximumLocationReached, c.DisplayName, c.MaxEquipedPerLocation, location.LocationName));
                    else
                        return new ValidateDropError(string.Format(c.AddAlreadyEquipedLocation, c.DisplayName, location.LocationName));

            }

            return last_result;
        }
    }
}