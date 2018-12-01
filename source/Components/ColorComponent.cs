using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    /// <summary>
    /// component has specific color
    /// </summary>
    [CustomComponent("Color")]
    public class ColorComponent : SimpleCustomComponent, IColorComponent
    {
        /// <summary>
        /// color of component
        /// </summary>
        public UIColor UIColor { get; set; }

        [JsonIgnore]
        public Color RGBColor => Color.black;
    }
}
