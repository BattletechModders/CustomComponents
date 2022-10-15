using System.Collections.Generic;

namespace CustomComponents;

internal class CategoryDefaultCustomsPreProcessor : IPreProcessor
{
    public void PreProcess(object target, Dictionary<string, object> values)
    {
        if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
        {
            return;
        }

        if (!(customSettingsObject is Dictionary<string, object> customSettings))
        {
            return;
        }

        if (!customSettings.TryGetValue(Category.CategoryCustomName, out var categoryCustomObject))
        {
            return;
        }

        foreach (var categoryID in GetCategoryIDs(categoryCustomObject))
        {
            var categoryDescriptor = CategoryController.Shared.GetOrCreateCategory(categoryID);
            if (categoryDescriptor.DefaultCustoms == null)
            {
                continue;
            }

            Control.LogDebug(DType.CCLoading, $"--copying defaults from category {categoryID}");
            foreach (var kv in categoryDescriptor.DefaultCustoms)
            {
                if (customSettings.ContainsKey(kv.Key))
                {
                    continue;
                }
                Control.LogDebug(DType.CCLoading, $"--copying {kv.Key} from category {categoryID}");
                customSettings[kv.Key] = kv.Value;
            }
        }
    }

    private IEnumerable<string> GetCategoryIDs(object categoryCustomObject)
    {
        if (categoryCustomObject is List<object> categoryObjectList)
        {
            foreach (var categoryObject in categoryObjectList)
            {
                if (categoryObject is Dictionary<string, object> categoryDict)
                {
                    if (categoryDict.TryGetValue(nameof(Category.CategoryID), out var categoryIDObject) && categoryIDObject is string categoryID)
                    {
                        yield return categoryID;
                    }
                }
            }
        }
        else if (categoryCustomObject is Dictionary<string, object> categoryDict)
        {
            if (categoryDict.TryGetValue(nameof(Category.CategoryID), out var categoryIDObject) && categoryIDObject is string categoryID)
            {
                yield return categoryID;
            }
        }
    }
}
