using System;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    public interface ICategory
    {
        string Category { get; }
        string MixCategory { get; }

        [JsonIgnore]
        CategoryDescriptor CategoryDescriptor { get; set; }
    }

    public static class CategoryExtensions
    {
        public static string GetMixCategory(this ICategory item)
        {
            if(item == null)
                return String.Empty;

            if (string.IsNullOrEmpty(item.MixCategory))
            {
                return item is MechComponentDef def ? def.Description.Id : string.Empty;
            }

            return item.MixCategory;
        }
    }
}