using BattleTech;
using BattleTech.UI;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{

    public class CustomJumpJetDef<T> : BattleTech.JumpJetDef, ICustomComponent
        where T : CustomJumpJetDef<T>
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

        public Type TooltipType => TooltipHelper.GetTooltipType(new JumpJetDef());
    }

    [Custom("CategoryJumpJet")]
    public class CategoryCustomJumpJetDef : CustomJumpJetDef<CategoryCustomJumpJetDef>, ICategory
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("ColorCategoryJumpJet")]
    public class ColorCategoryCustomJumpJetDef : CustomJumpJetDef<ColorCategoryCustomJumpJetDef>, ICategory, IColorComponent
    {
        public string CategoryID { get; set; }
        public string Tag { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public UIColor Color { get; set; }
    }

    [Custom("ColorJumpJet")]
    public class ColorCustomJumpJetDef : CustomJumpJetDef<ColorCustomJumpJetDef>, IColorComponent
    {
        public UIColor Color { get; set; }
    }

}
