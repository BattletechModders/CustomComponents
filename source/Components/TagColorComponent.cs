using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [CustomComponent("ColorTag", group: "ColorType")]
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


    [CustomComponent("TColorTag", group: "TColorType")]
    public class TTagColorComponent : SimpleCustomComponent, ITColorComponent, IAfterLoad
    {
        [JsonIgnore] public UIColor UIColor => UIColor.Custom;
        [JsonIgnore]
        public Color RGBColor { get; private set; }

        public bool SkipIcon { get; set; } = false;
        public bool SkipText { get; set; } = false;

        public string Tag { get; set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            if (Control.Settings.ColorTagsDictionary.TryGetValue(Tag, out var color))
                RGBColor = color;
            else
                RGBColor = Color.white;
        }
    }
}