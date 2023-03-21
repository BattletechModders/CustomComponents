using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using HBS;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;
using Text = Localize.Text;

namespace CustomComponents;

public abstract class HardpointHelper
{
    public HardpointInfo HPInfo { get; protected set; }


    public abstract void Hide();
    public abstract void Show();


    protected UIColor uicolor;
    protected Color color;
    protected Color backcolor;


    public LocalizableText Text { get; protected set; }
    public SVGImage Icon { get; protected set; }
    public Image BackImage { get; protected set; }
    public UIColorRefTracker TextColor { get; protected set; }
    public UIColorRefTracker IconColor { get; protected set; }
    public HBSTooltip Tooltip { get; protected set; }

    protected void init(SVGAsset image, string caption, string tooltip)
    {
        Icon.vectorGraphics = image;
        Text.SetText("-");

        if (Control.Settings.ColorHardpointsBack)
        {
            if (uicolor != UIColor.Custom)
            {
                backcolor = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.GetUIColor(uicolor);
            }
            else
            {
                backcolor = color;
            }

            backcolor.a = Control.Settings.HardpointBackAlpha;
            if (BackImage != null)
            {
                BackImage.color = backcolor;
            }
        }

        SetIconColor();
        SetTextColor();
        SetTooltip(caption, tooltip);
        Show();
    }

    protected virtual void SetTooltip(string caption, string text)
    {
        if (Tooltip == null)
        {
            return;
        }

        var loctext = new Text(text);
        var captest = new Text(caption);

        var desc = new BaseDescriptionDef("hardpoint", captest.ToString(), loctext.ToString(),
            HPInfo == null ? "" : HPInfo.WeaponCategory.Icon);

        Tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(desc));
    }

    private void SetTextColor()
    {
        if (Control.Settings.ColorHardpointsText)
        {
            TextColor.SetUIColor(uicolor);
            if (uicolor == UIColor.Custom)
            {
                TextColor.OverrideWithColor(color);
            }
        }
        else
        {
            TextColor.SetUIColor(Control.Settings.HardpointTextDefaultColor);
        }
    }

    private void SetIconColor()
    {
        if (Control.Settings.ColorHardpointsIcon)
        {
            IconColor.SetUIColor(uicolor);
            if (uicolor == UIColor.Custom)
            {
                IconColor.OverrideWithColor(color);
            }
        }
        else
        {
            IconColor.SetUIColor(Control.Settings.HardpointIconDefaultColor);
        }
    }


    public virtual void SetText(int used, int max)
    {
        SetText($"{used}/{max}");
        if (used > max)
        {
            TextColor.SetUIColor(UIColor.Red);
        }
        else
        {
            SetTextColor();
        }
    }

    public virtual void SetText(int n)
    {
        Text.SetText(n.ToString());
    }

    public virtual void SetText(string txt)
    {
        Text.SetText(txt);
    }
}