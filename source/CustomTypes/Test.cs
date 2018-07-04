using BattleTech;
using BattleTech.UI;
using HBS.Util;

namespace CustomComponents
{
    [Custom("LegActuator")]
    public class LegActuator : CategoryCustomUpgradeDef
    {
        public override void FromJson(string json)
        {
            JSONSerializationUtility.FromJSON(this, json, null);
            if (base.statusEffects == null)
            {
                base.statusEffects = new EffectData[0];
            }
        }

        public override string ToJson()
        {
            return JSONSerializationUtility.ToJSON(this);
        }
    }

    [Custom("LegActuatorDefault")]
    public class LefActuatorDefault : LegActuator, IDefault, IColorComponent
    {
        public UIColor Color { get; set; }

        public override void FromJson(string json)
        {
            JSONSerializationUtility.FromJSON(this, json, null);
            if (base.statusEffects == null)
            {
                base.statusEffects = new EffectData[0];
            }
        }

        public override string ToJson()
        {
            return JSONSerializationUtility.ToJSON(this);
        }
    }

    [Custom("TestEngineS")]
    public class TestEngineShieldingDef : CustomHeatSinkDef<TestEngineShieldingDef>, IAutoLinked, ICategory
    {
        public Link[] Links { get; set; }
        public string CategoryID { get; set; }
        public string Tag { get; set;  }
        public CategoryDescriptor CategoryDescriptor { get; set; }
    }

    [Custom("TestEngineSDefault")]
    public class TestEngineShieldingSide : CustomHeatSinkDef<TestEngineShieldingSide>, IDefault
    {

    }
}
