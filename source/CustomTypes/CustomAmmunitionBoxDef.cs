using BattleTech;
using BattleTech.UI;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    public class CustomAmmunitionBoxDef<T> : BattleTech.AmmunitionBoxDef, ICustomComponent
        where T : CustomAmmunitionBoxDef<T>
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
}
