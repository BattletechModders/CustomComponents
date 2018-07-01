using BattleTech;
using BattleTech.UI;
using HBS.Util;

namespace CustomComponents
{

    public class CustomHeatSinkDef<T> : BattleTech.HeatSinkDef, ICustomComponent
        where T : CustomHeatSinkDef<T>
    {
        public string CustomType { get; set; }

        public virtual void FromJson(string json)
        {
            JSONSerializationUtility.FromJSON<T>(this as T, json, null);
            if (base.statusEffects == null)
            {
                base.statusEffects = new EffectData[0];
            }
        }

        public virtual string ToJson()
        {
            return JSONSerializationUtility.ToJSON<T>(this as T);
        }
    }

    [Custom("CategoryHeatSink")]
    public class CategoryCustomHeatSinkDef : CustomHeatSinkDef<CategoryCustomHeatSinkDef>, ICategory
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("ColorCategoryHeatSink")]
    public class ColorCategoryCustomHeatSinkDef : CustomHeatSinkDef<ColorCategoryCustomHeatSinkDef>, ICategory, IColorComponent
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }


    [Custom("ColorHeatSink")]
    public class ColorCustomHeatSinkDef : CustomHeatSinkDef<ColorCustomHeatSinkDef>, IColorComponent
    {
        public UIColor Color { get; set; }
    }

}
