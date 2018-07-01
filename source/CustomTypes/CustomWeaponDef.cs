using BattleTech;
using BattleTech.UI;
using HBS.Util;

namespace CustomComponents
{

    public class CustomWeaponDef<T> : BattleTech.WeaponDef, ICustomComponent
        where T : CustomWeaponDef<T>
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

    [Custom("CategoryWeapon")]
    public class CategoryCustomWeaponDef : CustomWeaponDef<CategoryCustomWeaponDef>, ICategory
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("ColorCategoryWeapon")]
    public class ColorCategoryCustomWeaponDef : CustomWeaponDef<ColorCategoryCustomWeaponDef>, ICategory, IColorComponent
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }


    [Custom("ColorWeapon")]
    public class ColorCustomWeaponDef : CustomWeaponDef<ColorCustomWeaponDef>, IColorComponent
    {
        public UIColor Color { get; set; }
    }

}
