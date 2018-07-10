using System;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    [CustomComponent("Category")]
    public class Category : SimpleCustomComponent, IAfterLoad
    {
        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        string Tag;

        public string GetTag()
        {
            if (string.IsNullOrEmpty(Tag))
                return Def.Description.Id;
            else
                return Tag;
        }

        [JsonIgnore]
        public CategoryDescriptor CategoryDescriptor { get; set; }

        public  void OnLoaded()
        {
            CategoryDescriptor = Control.GetCategory(CategoryID);
        }
    }
}