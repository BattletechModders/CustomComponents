using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [CustomComponent("ColorTag")]
    public class TagColorComponent : SimpleCustomComponent, IColorComponent, IAfterLoad
    {
        [JsonIgnore] public UIColor UIColor => UIColor.Custom;
        [JsonIgnore]
        public Color RGBColor { get; private set; }

        public string Tag { get; set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            if (Control.Settings.ColorTagsDictionary.TryGetValue(Tag, out var color))
                RGBColor = color;
            else
                RGBColor = Color.magenta;
        }
    }
}