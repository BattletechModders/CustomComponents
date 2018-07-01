using BattleTech;
using BattleTech.UI;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    /// <summary>
    /// Custom ammobox definition
    /// </summary>
    /// <typeparam name="T">Your type</typeparam>
    public class CustomAmmunitionBoxDef<T> : BattleTech.AmmunitionBoxDef, ICustomComponent
        where T : CustomAmmunitionBoxDef<T>
    {
        public string CustomType { get; set; }

        public virtual void FromJson(string json)
        {
            JSONSerializationUtility.FromJSON(this as T, json, null);
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

    [Custom("CategoryAmmunitionBox")]
    public class CategoryCustomAmmunitionBoxDef : CustomAmmunitionBoxDef<CategoryCustomAmmunitionBoxDef>, ICategory
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("ColorCategoryAmmunitionBox")]
    public class ColorCategoryCustomAmmunitionBoxDef : CustomAmmunitionBoxDef<ColorCategoryCustomAmmunitionBoxDef>, ICategory, IColorComponent
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }


    [Custom("ColorAmmunitionBox")]
    public class ColorCustomAmmunitionBoxDef : CustomAmmunitionBoxDef<ColorCustomAmmunitionBoxDef>, IColorComponent
    {
        public UIColor Color { get; set; }
    }

}
