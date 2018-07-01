using BattleTech;
using BattleTech.UI;
using HBS.Util;

namespace CustomComponents
{

    public class CustomUpgradeDef<T> : BattleTech.UpgradeDef, ICustomComponent
        where T : CustomUpgradeDef<T>
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

    [Custom("CategoryUpgrade")]
    public class CategoryCustomUpgradeDef : CustomUpgradeDef<CategoryCustomUpgradeDef>,  ICategory
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("ColorCategoryUpgrade")]
    public class ColorCategoryCustomUpgradeDef : CustomUpgradeDef<ColorCategoryCustomUpgradeDef>, ICategory, IColorComponent
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }


    [Custom("ColorUpgrade")]
    public class ColorCustomUpgradeDef : CustomUpgradeDef<ColorCustomUpgradeDef>, IColorComponent
    {
        public UIColor Color { get; set; }
    }


}
