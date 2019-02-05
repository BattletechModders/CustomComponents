using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public class CategoriesHandler
    {
        internal static CategoriesHandler Shared = new CategoriesHandler();
        
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
                c.Apply(category);
            }
            else
            {
#if CCDEBUG
                Control.Logger.LogDebug($"Adding new: {category.Name}");
#endif
                Categories.Add(category.Name, category);
                category.InitDefaults();
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
    }
}