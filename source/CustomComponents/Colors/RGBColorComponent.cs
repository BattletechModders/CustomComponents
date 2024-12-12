using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents;

[CustomComponent("RGBColor")]
public class RGBColorComponent : SimpleCustomComponent, IColorComponent, IValueComponent<string>
{
    [JsonIgnore]
    public UIColor UIColor => UIColor.Custom;
    [JsonIgnore]
    public Color RGBColor { get; set; }

    public void LoadValue(string value)
    {
        if (ColorUtility.TryParseHtmlString(value, out var color))
        {
            RGBColor = color;
        }
        else
        {
            RGBColor = Color.magenta;
        }
    }
}

[CustomComponent("TRGBColor")]
public class TRGBColorComponent : SimpleCustomComponent, ITColorComponent, IOnLoaded
{
    [JsonIgnore]
    public UIColor UIColor => UIColor.Custom;
    [JsonIgnore]
    public Color RGBColor { get; set; }

    public bool SkipIcon { get; set; } = false;
    public bool SkipText { get; set; } = false;

    public string Color { get; set; }

    public void OnLoaded()
    {
        if (ColorUtility.TryParseHtmlString(Color, out var color))
        {
            RGBColor = color;
        }
        else
        {
            RGBColor = UnityEngine.Color.magenta;
        }
    }
}