using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using SVGImporter;
using UnityEngine;

namespace CustomComponents
{
    public class JJHardpointHeler : HardpointHelper
    {
        private GameObject go;

        public JJHardpointHeler(GameObject jjgo)
        {
            go = jjgo;

            uicolor = UIColor.White;

            Text = jjgo.GetComponentInChildren<LocalizableText>();
            Icon = jjgo.GetComponentInChildren<SVGImage>();

            TextColor = Text.GetComponent<UIColorRefTracker>();
            IconColor = Icon.GetComponent<UIColorRefTracker>();

            Tooltip = jjgo.GetComponent<HBSTooltip>();

            init(Icon.vectorGraphics, Control.Settings.ToolTips.JJCaption, Control.Settings.ToolTips.JJTooltip);
        }

        public JJHardpointHeler(GameObject jjgo, Transform jj)
        {
            go = jjgo;

            uicolor = UIColor.White;

            Text = jjgo.GetComponentInChildren<LocalizableText>();
            Icon = jjgo.GetComponentInChildren<SVGImage>();

            TextColor = Text.GetComponent<UIColorRefTracker>();
            IconColor = Icon.GetComponent<UIColorRefTracker>();

            var icon = jj.GetComponentInChildren<SVGImage>();
            Tooltip = jjgo.GetComponent<HBSTooltip>();


            init(icon.vectorGraphics, Control.Settings.ToolTips.JJCaption, Control.Settings.ToolTips.JJTooltip);
        }
        public override void Hide()
        {
            if (go.activeSelf)
                go.SetActive(false);
        }

        public override void Show()
        {
            if (!go.activeSelf)
                go.SetActive(true);
        }
    }
}