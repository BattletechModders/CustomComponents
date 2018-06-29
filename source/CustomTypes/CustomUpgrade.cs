using BattleTech;
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

}
