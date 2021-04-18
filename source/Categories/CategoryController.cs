#undef CCDEBUG

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
        internal static CategoryController Shared = new CategoryController();

        private readonly Dictionary<string, CategoryDescriptor> Categories = new Dictionary<string, CategoryDescriptor>();

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var category in SettingsResourcesTools.Enumerate<CategoryDescriptor>("CCCategories", customResources))
            {
                AddCategory(category);
                Control.Log($"Category {category.Name}({category.DisplayName}) registred");
            }
        }

        private void AddCategory(CategoryDescriptor category)
        {
            if (Categories.TryGetValue(category.Name, out var c))
                c.Apply(category);
            else
                Categories[category.Name] = category;

            category.Init();
            if(Control.Settings.DEBUG_ShowLoadedCategory)
                Control.Log(category.ToString());
        }

        internal CategoryDescriptor GetOrCreateCategory(string name)
        {
            if (Categories.TryGetValue(name, out var c))
                return c;
            c = new CategoryDescriptor { Name = name };
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
                var record = category[mechDef];

                if (record == null || record.LocationLimits.Count == 0)
                    continue;

                items_by_category.TryGetValue(category, out var items);
                
                foreach(var pair in record.LocationLimits)
                {
                    if(pair.Value.Min > 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);
                        
                        if(count < pair.Value.Min)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateMinimum,
                                category.DisplayName, pair.Value.Min, count, mechDef.Description.UIName, 
                                mechDef.Description.Name, pair.Key == ChassisLocations.All ? "All Locations" : pair.Key.ToString()));
                    }

                    if(pair.Value.Max >= 0)
                    {
                        var count = items == null ? 0 : items.Where(i => pair.Key.HasFlag(i.location)).Sum(i => i.num);
                        
                        if (count > pair.Value.Max)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateMaximum,
                                category.DisplayName, pair.Value.Max, count, mechDef.Description.UIName,
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
                                pair.Key.DisplayName));
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

        public static void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state)
        {
        }
    }
}