using System;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    public interface ICategory
    {
        string CategoryID { get; }
        string Tag { get; }

        [JsonIgnore]
        CategoryDescriptor CategoryDescriptor { get; set; }
    }

    public static class CategoryExtensions
    {
        public static string GetCategoryTag(this ICategory item)
        {
            if(item == null)
                return String.Empty;

            if (string.IsNullOrEmpty(item.Tag))
            {
                return item is MechComponentDef def ? def.Description.Id : string.Empty;
            }

            return item.Tag;
        }
    }
}