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
            }
        }

        private void AddCategory(CategoryDescriptor category)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"Add Category: {category.Name}");
#endif
            if (Categories.TryGetValue(category.Name, out var c))
            {
#if CCDEBUG
                Control.Logger.LogDebug($"Already have, apply: {category.Name}");
#endif
                category.Init();
                c.Apply(category);
            }
            else
            {
#if CCDEBUG
                Control.Logger.LogDebug($"Adding new: {category.Name}");
#endif
                Categories[category.Name] = category;
                category.Init();
            }

#if CCDEBUG
            Control.Logger.LogDebug($"Current Categories");
            foreach (var categoryDescriptor in Categories)
            {
                Control.Logger.LogDebug($" - {categoryDescriptor.Value.Name}");
            }
#endif
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


        /// <summary>
        /// validate mech and fill errors
        /// </summary>
        /// <param name="errors">errors by category</param>
        /// <param name="validationLevel"></param>
        /// <param name="mechDef">mech to validate</param>
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
                    mix = item.GetTag(),
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());


            //fcache.Clear();

            //check each "required" category
            foreach (var category in GetCategories())
            {
                var record = category[mechDef];

                if (record == null || !record.Required)
                    continue;

                if (!items_by_category.ContainsKey(category) || items_by_category[category].Sum(i => i.num) < record.MinEquiped)
                    if (record.MinEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateRequred, category.DisplayName.ToUpper(), category.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(category.ValidateMinimum, category.DisplayName.ToUpper(), category.DisplayName, record.MinEquiped));
            }

            foreach (var pair in items_by_category)
            {
                var record = pair.Key[mechDef];
                if (record == null)
                    continue;
                var count = pair.Value.Sum(i => i.num);

                //check if too many items of same category
                if (record.MaxEquiped > 0 && count > record.MaxEquiped)
                    if (record.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, record.MaxEquiped)));

                //check if cateory mix tags
                if (!pair.Key.AllowMixTags)
                {
                    string first_tag = pair.Value
                        .Select(i => i.mix == null ? "" : i.mix)
                        .FirstOrDefault(i => i != "*");
                    if (first_tag != null)
                    {
                        bool mixed = pair.Value.Any(i => i.mix != "*" && i.mix != null && i.mix != first_tag);
                        if (mixed)
                        {
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(pair.Key.ValidateMixed,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                        }
                    }
                }

                // check count items per location
                if (record.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Sum(a => a.num));
                    if (max > record.MaxEquipedPerLocation)
                        if (record.MaxEquipedPerLocation == 1)
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(pair.Key.ValidateUniqueLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                        else
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(pair.Key.ValidateMaximumLocation,
                                pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, record.MaxEquipedPerLocation));
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
#if CCDEBUG
            Control.Logger.LogDebug($"- Category");
#endif
            //var items_by_category = (from item in mechDef.Inventory
            //                         let def = item.Def.GetComponent<Category>()
            //                         where def != null
            //                         select new
            //                         {
            //                             category = def.CategoryDescriptor,
            //                             itemdef = item.Def,
            //                             itemref = item,
            //                             mix = def.GetTag()
            //                         }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            var items_by_category = mechDef.Inventory
                .Select(item => new { item, def = item.Def.GetComponents<Category>() })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = @t.item.Def,
                    itemref = @t.item,
                    mix = item.GetTag(),
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key, i => i.ToList());

            // if all required category present
            foreach (var category in GetCategories())
            {
                var record = category[mechDef];
                if (record == null || !record.Required)
                    continue;


#if CCDEBUG
                Control.Logger.LogDebug($"-- MinEquiped for {category.displayName}");
#endif


                if (!items_by_category.ContainsKey(category) || items_by_category[category].Sum(i => i.num) < record.MinEquiped)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"--- not passed {items_by_category[category].Count}/{category.MinEquiped}");
#endif
                    return false;
                }
            }

            foreach (var pair in items_by_category)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"-- MaxEquiped for {pair.Key.displayName}");
#endif                
                var record = pair.Key[mechDef];
                if (record == null)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"-- no record for unittype - skipped");
#endif
                    continue;
                }
                var count = pair.Value.Sum(i => i.num);

                //check if too many items of same category
                if (record.MaxEquiped > 0 && count > record.MaxEquiped)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"--- not passed {pair.Value.Count}/{pair.Key.MaxEquiped}");
#endif
                    return false;
                }

                //if mixed
                if (!pair.Key.AllowMixTags)
                {
                    string first_tag = pair.Value
                        .Select(i => i.mix == null ? "" : i.mix)
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

                // if too many per location
                if (record.MaxEquipedPerLocation > 0)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"-- MaxEquipedPerLocation for {pair.Key.displayName}");
#endif
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Sum(a => a.num));
                    if (max > record.MaxEquipedPerLocation)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- not passed {max}/{pair.Key.MaxEquipedPerLocation}");
#endif
                        return false;
                    }
                }
            }
#if CCDEBUG
            Control.Logger.LogDebug($"--- all passed");
#endif
            return true;
        }

        public bool need_remove(MechComponentRef item, MechDef mech)
        {
            if (!item.IsFixed)
                return false;

            if (item.IsModuleFixed(mech))
                return false;

            if (!item.IsDefault())
                return false;

            if (item.Is<Category>(out var category))
            {
                var record = category.CategoryDescriptor[mech];
                if (record.MaxEquiped > 0 || record.MaxEquipedPerLocation > 0)
                    return true;
            }

            return false;
        }

        public void RemoveExcessDefaults(List<MechDef> mechDefs, SimGameState state)
        {
            foreach (var mechDef in mechDefs)
            {
                mechDef.SetInventory(mechDef.Inventory.Where(i => !need_remove(i, mechDef)).ToArray());
                mechDef.Refresh();
            }
        }
    }
}