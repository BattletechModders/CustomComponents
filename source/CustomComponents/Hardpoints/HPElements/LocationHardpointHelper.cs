using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using UnityEngine;

namespace CustomComponents;

public class LocationHardpointHelper : HardpointHelper
{
    public CanvasGroup Canvas { get; protected set; }
    public MechLabHardpointElement Element { get; private set; }
    public WeaponCategoryValue WeaponCategory { get; private set; }

    public LocationHardpointHelper(MechLabHardpointElement element)

    {
        Element = element;
        WeaponCategory = null;

        Text = Element.hardpointText;
        TextColor = Text.GetComponent<UIColorRefTracker>();
        Icon = Element.hardpointIcon;
        IconColor = Icon.GetComponent<UIColorRefTracker>();
        Canvas = Element.thisCanvasGroup;
        Tooltip = element.GetComponent<HBSTooltip>();
        BackImage = null;
    }

    public virtual void Init(HardpointInfo hpinfo)
    {
        HPInfo = hpinfo;

        if (hpinfo?.WeaponCategory == null || hpinfo.WeaponCategory.Is_NotSet || !hpinfo.Visible)
        {
            Hide();
            return;
        }

        WeaponCategory = hpinfo.WeaponCategory;

        if (HPInfo.OverrideColor)
        {
            color = HPInfo.HPColor;
            uicolor = UIColor.Custom;
        }
        else
        {
            uicolor = HPInfo.WeaponCategory.GetUIColor();
        }

        init(HPInfo.WeaponCategory.GetIcon(), hpinfo.TooltipCaption, hpinfo.Description);
    }

    public override void Hide()
    {
        Canvas.alpha = 0f;
    }

    public override void Show()
    {
        Canvas.alpha = 1f;
    }
}