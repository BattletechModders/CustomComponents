using BattleTech.UI;

namespace CustomComponents
{
    [Custom("CategoryTest")]
    public class CategoryTestItem : CustomUpgradeDef<CategoryTestItem>, ICategory, IColorComponent
    {
        public string Category { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }
}