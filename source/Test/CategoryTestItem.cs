using BattleTech.UI;

namespace CustomComponents
{
    [Custom("CategoryTest")]
    public class CategoryTestItem : CustomUpgradeDef<CategoryTestItem>, ICategory, IColorComponent
    {
        public string Category { get; set; }
        public string MixCategory { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }

    [Custom("CategoryHeatSink")]
    public class CategoryHeatSinkDef : CustomHeatSinkDef<CategoryHeatSinkDef>, ICategory
    {
        public string Category { get; set; }
        public string MixCategory { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }
}