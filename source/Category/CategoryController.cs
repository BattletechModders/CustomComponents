using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public enum CategoryError
    {
        None,
        AreadyEquiped,
        MaximumReached,
        AlreadyEquipedLocation,
        MaximumReachedLocation,
        AllowMix
    }

    internal static class CategoryController
    {
        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            var items_by_category = (from item in mechDef.Inventory
                                     where item.Def is ICategory
                                     let def = item.Def as ICategory
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item,
                                         mix = def.GetCategoryTag()
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

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
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    if (pair.Key.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquiped));

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

        internal static CategoryError ValidateAdd(ICategory component, MechLabLocationWidget widget,
            MechLabPanel mechlab, out int count, out string location)
        {
            var category = component.CategoryDescriptor;

            var items = mechlab.activeMechDef.Inventory
                .Where(i => (i.Def as ICategory)?.CategoryDescriptor == category).ToList();

            count = 0;
            location = "";

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

            if (category.MaxEquipedPerLocation > 0)
            {
                int count_per_location = items.Count(i => i.MountedLocation == widget.loadout.Location);
                if (count_per_location >= category.MaxEquipedPerLocation)
                {
                    var helper = new LocationHelper(widget);
                    location = helper.LocationName;
                    if (category.UniqueForLocation)
                        return CategoryError.AlreadyEquipedLocation;

                    count = count_per_location;
                    return CategoryError.MaximumReachedLocation;
                }
            }

            if (!category.AllowMixTags)
            {
                if (items.Any(i => (i.Def as ICategory).GetCategoryTag() != component.GetCategoryTag()))
                    return CategoryError.AllowMix;
            }

            return CategoryError.None;
        }

        internal static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget,
            bool current_result, ref string errorMessage, MechLabPanel mechlab)
        {
            if (!current_result && !errorMessage.EndsWith("Not enough free slots."))
            {
                return false;
            }

            if (!(component is ICategory))
                return current_result;

            var error = ValidateAdd(component as ICategory, widget, mechlab, out var count, out var location_name);

            if (error == CategoryError.None)
                return current_result;

            var category = ((ICategory)component).CategoryDescriptor;
            var state = new CategoryValidatorState
            {
                Error = error,
                NotEnoughSlots = current_result,
                descriptor = category
            };

            Validator.AddState(state);

            if (category.AutoReplace &&
                (error == CategoryError.MaximumReached || error == CategoryError.MaximumReachedLocation))
            {
                var helper = new LocationHelper(widget);
                state.ReplacementIndex =
                    helper.LocalInventory.FindIndex(i => (i.Def is ICategory cat) && cat.CategoryID == category.Name);
                if (state.ReplacementIndex >= 0)
                {
                    state.Replacement = helper.LocalInventory[state.ReplacementIndex].Def;
                    // if not enough slot to replace - it also not enough slot to fit, so Not Enough Slots message will pop up
                    return helper.UsedSlots - state.Replacement.InventorySize + component.InventorySize <= helper.MaxSlots;
                }
            }

            switch (error)
            {
                case CategoryError.AreadyEquiped:
                    errorMessage = string.Format(category.AddAlreadyEquiped, category.DisplayName);
                    return false;
                case CategoryError.MaximumReached:
                    errorMessage = string.Format(category.AddMaximumReached, category.DisplayName, count);
                    return false;
                case CategoryError.AlreadyEquipedLocation:
                    errorMessage = string.Format(category.AddAlreadyEquipedLocation, category.DisplayName, location_name);
                    return false;
                case CategoryError.MaximumReachedLocation:
                    errorMessage = string.Format(category.AddMaximumLocationReached, category.DisplayName, location_name, location_name);
                    return false;
                case CategoryError.AllowMix:
                    errorMessage = string.Format(category.AddMixed, category.DisplayName);
                    return false;
                default:
                    return true;
            }
        }


        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            var items_by_category = (from item in mechDef.Inventory
                                     where item.Def is ICategory
                                     let def = item.Def as ICategory
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item,
                                         mix = def.GetCategoryTag()
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            foreach (var category in Control.GetCategories().Where(i => i.Required))
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    return false;

            foreach (var pair in items_by_category)
            {
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    return false;

                if (!pair.Key.AllowMixTags)
                {
                    string def = pair.Value[0].mix;

                    bool flag = pair.Value.Any(i => i.mix != def);
                    if (flag) return false;
                }

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