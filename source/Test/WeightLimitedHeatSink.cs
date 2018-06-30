using BattleTech.UI;

namespace CustomComponents
{
    [Custom("WeightHeatSink")]
    public class WeightHeatSinkDef : CustomHeatSinkDef<WeightHeatSinkDef>, IWeightLimited, IColorComponent
    {
        public int MinTonnage { get; set; }
        public int MaxTonnage { get; set; }
        public UIColor Color { get; set; }
    }
}