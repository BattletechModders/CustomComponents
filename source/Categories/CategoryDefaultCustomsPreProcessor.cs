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

        foreach (var categoryID in GetCategoryIDs(customSettings))
        {
            var categoryDescriptor = CategoryController.Shared.GetOrCreateCategory(categoryID);
            if (categoryDescriptor.DefaultCustoms == null)
            {
                return;
            }

            foreach (var kv in categoryDescriptor.DefaultCustoms)
            {
                if (customSettings.ContainsKey(kv.Key))
                {
                    continue;
                }
                customSettings[kv.Key] = kv.Value;
            }
        }
    }

    private IEnumerable<string> GetCategoryIDs(Dictionary<string, object> customSettings)
    {
        if (!customSettings.TryGetValue(Category.CategoryCustomName, out var categoryCustomObject))
        {
            yield break;
        }

        if (categoryCustomObject is List<object> categoryObjectList)
        {
            foreach (var categoryObject in categoryObjectList)
            {
                if (categoryObject is Dictionary<string, object> categoryDict)
                {
                    if (categoryDict.TryGetValue(nameof(Category.CategoryID), out var categoryID))
                    {
                        yield return categoryID as string;
                    }
                }
            }
        }
        else if (categoryCustomObject is Dictionary<string, object> categoryDict)
        {
            if (categoryDict.TryGetValue(nameof(Category.CategoryID), out var categoryID))
            {
                yield return categoryID as string;
            }
        }
    }
}
