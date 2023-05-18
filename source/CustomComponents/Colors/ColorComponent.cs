using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents;

/// <summary>
/// component has specific color
/// </summary>
[CustomComponent("Color")]
public class ColorComponent : SimpleCustomComponent, IColorComponent, IValueComponent<UIColor>
{
    /// <summary>
    /// color of component
    /// </summary>
    public UIColor UIColor { get; set; } = UIColor.Red;

    [JsonIgnore]
    public Color RGBColor => Color.black;

    public void LoadValue(UIColor value)
    {
        UIColor = value;
    }
}


/// <summary>
/// component has specific color
/// </summary>
[CustomComponent("TColor")]
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