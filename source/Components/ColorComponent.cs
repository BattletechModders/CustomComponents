using System;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    /// <summary>
    /// component has specific color
    /// </summary>
    [CustomComponent("Color", group: "ColorType")]
    public class ColorComponent : SimpleCustomComponent, IColorComponent, IValueComponent
    {
        /// <summary>
        /// color of component
        /// </summary>
        public UIColor UIColor { get; set; } = UIColor.Red;

        [JsonIgnore]
        public Color RGBColor => Color.black;

        public void LoadValue(object value)
        {
            if (value is string str && Enum.TryParse(str, true, out UIColor color))
            {
                UIColor = color;
            }
        }
    }


    /// <summary>
    /// component has specific color
    /// </summary>
    [CustomComponent("TColor", group: "TColorType")]
    public class TColorComponent : SimpleCustomComponent, ITColorComponent
    {
        /// <summary>
        /// color of component
        /// </summary>
        public UIColor UIColor { get; set; }

        [JsonIgnore]
        public Color RGBColor => Color.white;

        public bool SkipIcon { get; set; } = false;
        public bool SkipText { get; set; } = false;
    }
}
