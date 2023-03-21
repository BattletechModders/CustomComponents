using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents;

public class JJHardpointHeler : HardpointHelper
{
    private GameObject go;

    public JJHardpointHeler(GameObject jjgo, Transform jj)
    {
        go = jjgo;

        uicolor = Control.Settings.HardpointJJTextAndIconColor;

        Text = jjgo.GetComponentInChildren<LocalizableText>();
        Icon = jjgo.GetComponentInChildren<SVGImage>();
        BackImage = jjgo.GetComponent<Image>();


        TextColor = Text.GetComponent<UIColorRefTracker>();
        IconColor = Icon.GetComponent<UIColorRefTracker>();

        var icon = jj.GetComponentInChildren<SVGImage>();
        Tooltip = jjgo.GetComponent<HBSTooltip>();


        init(icon.vectorGraphics, Control.Settings.ToolTips.JJCaption, Control.Settings.ToolTips.JJTooltip);
    }
    public override void Hide()
    {
        if (go.activeSelf)
        {
            go.SetActive(false);
        }
    }

    public override void Show()
    {
        if (!go.activeSelf)
        {
            go.SetActive(true);
        }
    }
}