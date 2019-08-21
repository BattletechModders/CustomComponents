using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [CustomComponent("RGBColor", group: "ColorType")]
    public class RGBColorComponent : SimpleCustomComponent, IColorComponent, IAfterLoad
    {
        [JsonIgnore]
        public UIColor UIColor => UIColor.Custom;
        [JsonIgnore]
        public Color RGBColor { get; set; }

        public string Color { get; set; }


        public void OnLoaded(Dictionary<string, object> values)
        {
            if (ColorUtility.TryParseHtmlString(Color, out var color))
            {
                RGBColor = color;
            }
            else
                RGBColor = UnityEngine.Color.magenta;
        }
    }

    [CustomComponent("TRGBColor", group: "TColorType")]
    public class TRGBColorComponent : SimpleCustomComponent, ITColorComponent, IAfterLoad
    {
        [JsonIgnore]
        public UIColor UIColor => UIColor.Custom;
        [JsonIgnore]
        public Color RGBColor { get; set; }

        public bool SkipIcon { get; set; } = false;
        public bool SkipText { get; set; } = false;

        public string Color { get; set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            if (ColorUtility.TryParseHtmlString(Color, out var color))
            {
                RGBColor = color;
            }
            else
                RGBColor = UnityEngine.Color.magenta;
        }
    }
}