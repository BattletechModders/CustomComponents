#undef CCDEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// error for category check
    /// </summary>
    //internal enum CategoryError
    //{
    //    None,
    //    AreadyEquiped,
    //    MaximumReached,
    //    AlreadyEquipedLocation,
    //    MaximumReachedLocation,
    //    AllowMix
    //}

    /// <summary>
    /// class to handle category interaction
    /// </summary>
    public class CategoryController
    {
        public static CategoryController Shared = new();

        private readonly Dictionary<string, CategoryDescriptor> Categories = new Dictionary<string, CategoryDescriptor>();

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var category in SettingsResourcesTools.Enumerate<CategoryDescriptor>("CCCategories", customResources))
            {
                AddCategory(category);
                Control.Log($"Category {category.Name}({category._DisplayName}) registered");
            }
        }

        private void AddCategory(CategoryDescriptor category)
        {
            if (Categories.TryGetValue(category.Name, out var c))
            {
                Control.Log("already have " + c.ToString());
                c.Apply(category);
                category = c;
            }
            else
                Categories[category.Name] = category;

            category.Init();
            if (Control.Settings.DEBUG_ShowLoadedCategory)
                Control.Log(category.ToString());
        }

        internal CategoryDescriptor GetOrCreateCategory(string name)
        {
            if (Categories.TryGetValue(name, out var c))
                return c;
            c = new CategoryDescriptor { Name = name };
            //Control.Log("create empty " + name + " for " + Environment.StackTrace);
            Categories.Add(name, c);
            return c;
        }

        internal CategoryDescriptor GetCategory(string name)
        {
            return Categories.TryGetValue(name, out var c) ? c : null;
        }

        internal IEnumerable<CategoryDescriptor> GetCategories()
        {
            return Categories.Values;
        }


        internal void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {

            var items_by_category = mechDef.Inventory
                .Select(item => new { item, def = item.Def.GetComponents<Category>() })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = @t.item.Def,
                    itemref = @t.item,
                    location = t.item.MountedLocation,
                    mix = item.GetTag(),
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());



            //check all minimum values
            foreach (var category in GetCategories())
            {
                CategoryDescriptorRecord record;

                try
                {
                    record = category[mechDef];
                }
                catch (NullReferenceException nre)
                {
                    Control.LogError($"mech: {mechDef.Description.Id}, category: {category.Name}", nre);
                    continue;

                }

                if (record == null || record.LocationLimits.Count == 0)
                    continue;

                items_by_category.TryGetValue(category, out var items);

                foreach (var pair in record.LocationLimits)
                {
                    if (pair.Value.Min > 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count < pair.Value.Min)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateMinimum,
                                category._DisplayName, pair.Value.Min, count, mechDef.Description.UIName,
                                mechDef.Description.Name, pair.Key == ChassisLocations.All ? "All Locations" : pair.Key.ToString()));
                    }

                    if (pair.Value.Max >= 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count > pair.Value.Max)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateMaximum,
                                category._DisplayName, pair.Value.Max, count, mechDef.Description.UIName,
                                mechDef.Description.Name, pair.Key == ChassisLocations.All ? "All Locations" : pair.Key.ToString()));
                    }
                }

            }

            foreach (var pair in items_by_category)
            {
                var record = pair.Key[mechDef];
                if (record == null)
                    continue;

                //check if cateory mix tags
                if (!pair.Key.AllowMixTags)
                {
                    string first_tag = pair.Value
                        .Select(i => i.mix == null ? "!null!" : i.mix)
                        .FirstOrDefault(i => i != "*");

                    if (first_tag != null)
                    {
                        bool mixed = pair.Value.Any(i => i.mix != "*" && i.mix != null && i.mix != first_tag);
                        if (mixed)
                        {
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(pair.Key.ValidateMixed,
                                pair.Key._DisplayName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check if mech can be fielded
        /// </summary>
        /// <param name="mechDef"></param>
        /// <returns></returns>
        internal bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            var items_by_category = mechDef.Inventory
                .Select(item => new { item, def = item.Def.GetComponents<Category>() })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = @t.item.Def,
                    itemref = @t.item,
                    location = t.item.MountedLocation,
                    mix = item.GetTag(),
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());



            //check all minimum values
            foreach (var category in GetCategories())
            {
                var record = category[mechDef];

                if (record == null || record.LocationLimits.Count == 0)
                    continue;

                items_by_category.TryGetValue(category, out var items);

                foreach (var pair in record.LocationLimits)
                {
                    if (pair.Value.Min > 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count < pair.Value.Min)
                            return false;
                    }

                    if (pair.Value.Max >= 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count > pair.Value.Max)
                            return false;
                    }
                }

            }

            foreach (var pair in items_by_category)
            {
                var record = pair.Key[mechDef];
                if (record == null)
                    continue;

                //check if cateory mix tags
                if (!pair.Key.AllowMixTags)
                {
                    string first_tag = pair.Value
                        .Select(i => i.mix == null ? "!null!" : i.mix)
                        .FirstOrDefault(i => i != "*");

                    if (first_tag != null)
                    {
                        bool mixed = pair.Value.Any(i => i.mix != "*" && i.mix != null && i.mix != first_tag);
                        if (mixed)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public string ValidateDrop(MechLabItemSlotElement drop_item, List<InvItem> new_inventory)
        {
            var items_by_category = new_inventory
                .Select(iitem => new { iitem, def = iitem.Item.Def.GetComponents<Category>() })
                .Where(i => i.def != null)
                .SelectMany(i => i.def.Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = i.iitem.Item.Def,
                    itemref = i.iitem,
                    location = i.iitem.Location,
                    mix = item.GetTag(),
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());

            var mechDef = MechLabHelper.CurrentMechLab.ActiveMech;

            //check all minimum values
            foreach (var category in GetCategories())
            {
                CategoryDescriptorRecord record;

                if (category.AllowMinOverflow && category.AllowMaxOverflow)
                    continue;

                try
                {
                    record = category[mechDef];
                }
                catch (NullReferenceException nre)
                {
                    Control.LogError($"mech: {mechDef.Description.Id}, category: {category.Name}", nre);
                    continue;

                }

                if (record == null || record.LocationLimits.Count == 0)
                    continue;

                items_by_category.TryGetValue(category, out var items);

                foreach (var pair in record.LocationLimits)
                {
                    if (!category.AllowMinOverflow && pair.Value.Min > 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count < pair.Value.Min)
                            return (new Localize.Text(category.ValidateMinimum,
                                 category._DisplayName, pair.Value.Min, count, mechDef.Description.UIName,
                                 mechDef.Description.Name, pair.Key == ChassisLocations.All ? "All Locations" : pair.Key.ToString())).ToString();
                    }

                    if (!category.AllowMaxOverflow && pair.Value.Max >= 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);

                        if (count > pair.Value.Max)
                            return (new Localize.Text(category.ValidateMaximum,
                                 category._DisplayName, pair.Value.Max, count, mechDef.Description.UIName,
                                 mechDef.Description.Name, pair.Key == ChassisLocations.All ? "All Locations" : pair.Key.ToString())).ToString();
                    }
                }

            }

            foreach (var pair in items_by_category)
            {
                var record = pair.Key[mechDef];
                if (record == null)
                    continue;

                //check if cateory mix tags
                if (pair.Key.AllowMixTags || pair.Key.AllowMixTagsMechlab)
                    continue;

                string first_tag = pair.Value
                    .Select(i => i.mix ?? "*")
                    .FirstOrDefault(i => i != "*");

                if (first_tag != null)
                {
                    bool mixed = pair.Value.Any(i => i.mix != "*" && i.mix != null && i.mix != first_tag);
                    if (mixed)
                    {
                        return (new Localize.Text(pair.Key.ValidateMixed,
                            pair.Key._DisplayName)).ToString();
                    }
                }
            }

            return string.Empty;
        }
    }
}