using System;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    public interface ICategory
    {
        /// <summary>
        /// name of category
        /// </summary>
        string CategoryID { get; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        string Tag { get; }

        [JsonIgnore]
        CategoryDescriptor CategoryDescriptor { get; set; }
    }

    /// <summary>
    /// extention methods for category
    /// </summary>
    public static class CategoryExtensions
    {
        /// <summary>
        /// return tag for allow mixing
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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