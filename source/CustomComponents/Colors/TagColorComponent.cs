using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents;

[CustomComponent("ColorTag")]
public class TagColorComponent : SimpleCustomComponent, IColorComponent, IValueComponent<string>
{
    [JsonIgnore] public UIColor UIColor => UIColor.Custom;
    [JsonIgnore]
    public Color RGBColor { get; private set; }

    public void LoadValue(string value)
    {
        if (Control.Settings.ColorTagsDictionary.TryGetValue(value, out var color))
        {
            RGBColor = color;
        }
        else
        {
            RGBColor = Color.magenta;
        }
    }
}


[CustomComponent("TColorTag")]
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
        {
            RGBColor = color;
        }
        else
        {
            RGBColor = Color.white;
        }
    }
}