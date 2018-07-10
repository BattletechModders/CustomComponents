using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// error for category check
    /// </summary>
    internal enum CategoryError
    {
        None,
        AreadyEquiped,
        MaximumReached,
        AlreadyEquipedLocation,
        MaximumReachedLocation,
        AllowMix
    }

    /// <summary>
    /// class to handle category interaction
    /// </summary>
    internal static class CategoryController
    {
        /// <summary>
        /// validate mech and fill errors
        /// </summary>
        /// <param name="errors">errors by category</param>
        /// <param name="validationLevel"></param>
        /// <param name="mechDef">mech to validate</param>
        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {

            var items_by_category = (from item in mechDef.Inventory
                                     let def = item.Def.GetComponent<Category>()
                                     where def != null
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item,
                                         mix = def.GetTag()
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            //check each "required" category
            foreach (var category in Control.GetCategories().Where(i => i.Required))
            {
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    if (category.MinEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateRequred, category.DisplayName.ToUpper(), category.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateMinimum, category.DisplayName.ToUpper(), category.DisplayName, category.MinEquiped));
            }

            foreach (var pair in items_by_category)
            {
                //check if too mant items of same category
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    if (pair.Key.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquiped));

                //check if cateory mix tags
                if (!pair.Key.AllowMixTags)
                {
                    string def = pair.Value[0].mix;

                    bool flag = pair.Value.Any(i => i.mix != def);
                    if (flag)
                    {
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMixed,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    }
                }

                // check count items per location
                if (pair.Key.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if (max > pair.Key.MaxEquipedPerLocation)
                        if (pair.Key.MaxEquipedPerLocation == 1)
                            errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUniqueLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                        else
                            errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximumLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquipedPerLocation));
                }
            }
        }

        // return first error for validate drop
        internal static CategoryError ValidateAdd(Category component, LocationHelper location, out int count)
        {
            var category = component.CategoryDescriptor;

            var items = location.mechLab.activeMechDef.Inventory
                .Where(i => i.Is<Category>(out var c) && c.CategoryID == category.Name).ToList();

            count = 0;

            //too many
            if (category.MaxEquiped > 0)
            {
                if (items.Count >= category.MaxEquiped)
                {
                    if (category.Unique)
                        return CategoryError.AreadyEquiped;
                    count = items.Count;
                    return CategoryError.MaximumReached;
                }
            }

            // to many per location
            if (category.MaxEquipedPerLocation > 0)
            {
                int count_per_location = items.Count(i => i.MountedLocation == location.widget.loadout.Location);
                if (count_per_location >= category.MaxEquipedPerLocation)
                {
                    if (category.UniqueForLocation)
                        return CategoryError.AlreadyEquipedLocation;

                    count = count_per_location;
                    return CategoryError.MaximumReachedLocation;
                }
            }

            //mixed tags
            if (!category.AllowMixTags)
            {
                if (items.Any(i => i.Is<Category>(out var c) && c.GetTag() != component.GetTag()))
                    return CategoryError.AllowMix;
            }

            return CategoryError.None;
        }

        /// <summary>
        /// validate drop
        /// </summary>
        /// <param name="element">item to drop</param>
        /// <param name="location">where to drop</param>
        /// <param name="last_result"></param>
        /// <returns></returns>
        internal static IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {

            Control.Logger.LogDebug($"ICategory validation start for {location.widget.loadout?.Location.ToString() ?? "???"}");

            var component = element.ComponentRef.Def;

            //if not a category - skip category check
            if (!component.Is<Category>(out var cat_component))
            {
                Control.Logger.LogDebug("Not a category");
                return last_result;
            }
            
            //get error
            var error = ValidateAdd(cat_component, location, out var count);
            
            Control.Logger.LogDebug($"Category: {cat_component.CategoryID}, Error: {error}");

            //if no errors = all ok, return
            if (error == CategoryError.None)
                return last_result;

            var category = cat_component.CategoryDescriptor;

            Control.Logger.LogDebug($"Category: Validator state create");

            //check if can replace item to override "to many"
            if (category.AutoReplace && error != CategoryError.AllowMix)
            {
                Control.Logger.LogDebug($"Category: Search for repacement");

                var replacement = location.LocalInventory.FirstOrDefault(e =>
                    e.ComponentRef.Is<Category>(out var cat) && cat.CategoryID == category.Name);

                if (replacement != null)
                {
                    last_result = ValidateDropChange.AddOrCreate(last_result,
                        new RemoveChange(location.widget.loadout.Location, replacement));


                    return last_result;

                }
            }

            Control.Logger.LogDebug($"Category: return error message");

            //return error message
            switch (error)
            {
                case CategoryError.AreadyEquiped:
                    return new ValidateDropError(string.Format(category.AddAlreadyEquiped, category.DisplayName));
                case CategoryError.MaximumReached:
                    return new ValidateDropError(string.Format(category.AddMaximumReached, category.DisplayName, count));
                case CategoryError.AlreadyEquipedLocation:
                    return new ValidateDropError(string.Format(category.AddAlreadyEquipedLocation, category.DisplayName, location.LocationName));
                case CategoryError.MaximumReachedLocation:
                    return new ValidateDropError(string.Format(category.AddMaximumLocationReached, category.DisplayName, count, location.LocationName));
                case CategoryError.AllowMix:
                    return new ValidateDropError(string.Format(category.AddMixed, category.DisplayName));
            }

            return last_result;
        }

        /// <summary>
        /// check if mech can be fielded
        /// </summary>
        /// <param name="mechDef"></param>
        /// <returns></returns>
        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            var items_by_category = (from item in mechDef.Inventory
                                     let def = item.Def.GetComponent<Category>()
                                     where def != null
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item,
                                         mix = def.GetTag()
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            // if all required category present
            foreach (var category in Control.GetCategories().Where(i => i.Required))
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    return false;

            foreach (var pair in items_by_category)
            {
                // if too many equiped
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    return false;

                //if mixed
                if (!pair.Key.AllowMixTags)
                {
                    string def = pair.Value[0].mix;

                    bool flag = pair.Value.Any(i => i.mix != def);
                    if (flag) return false;
                }

                // if too many per location
                if (pair.Key.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if (max > pair.Key.MaxEquipedPerLocation)
                        return false;
                }
            }

            return true;
        }
    }
}