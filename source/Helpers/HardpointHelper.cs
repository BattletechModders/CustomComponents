using System;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomComponents.Patches;
using Harmony;
using Localize;
using SVGImporter;
using UnityEngine;
using UIColor = BattleTech.UI.UIColor;

namespace CustomComponents
{

    public abstract class HardpointHelper
    {
        public HardpointInfo HPInfo { get; protected set; }


        public abstract void Hide();
        public abstract void Show();


        protected UIColor uicolor;
        protected Color color;
        

        public LocalizableText Text { get; protected set; }
        public SVGImage Icon { get; protected set; }
        public UIColorRefTracker TextColor { get; protected set; }
        public UIColorRefTracker IconColor { get; protected set; }
        public HBSTooltip Tooltip { get; protected set; }

        protected void init(SVGAsset image, string caption, string tooltip)
        {
            Icon.vectorGraphics = image;
            Text.SetText("-");

            SetIconColor();
            SetTextColor();
            SetTooltip(caption,tooltip);
            Show();
        }

        protected virtual void SetTooltip(string caption, string text)
        {
            if (Tooltip == null)
                return;

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
                    TextColor.OverrideWithColor(color);
            }
            else
                TextColor.SetUIColor(UIColor.White);
        }

        private void SetIconColor()
        {
            if (Control.Settings.ColorHardpointsIcon)
            {
                IconColor.SetUIColor(uicolor);
                if (uicolor == UIColor.Custom)
                    IconColor.OverrideWithColor(color);
            }
            else
                IconColor.SetUIColor(UIColor.White);

        }


        public virtual void SetText(int used, int max)
        {
            SetText($"{used}/{max}");
            if (used > max)
                TextColor.SetUIColor(UIColor.Red);
            else
                SetTextColor();

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

    public class LocationHardpointHelper : HardpointHelper
    {
        public CanvasGroup Canvas { get; protected set; }
        public MechLabHardpointElement Element { get; private set; }
        public WeaponCategoryValue WeaponCategory { get; private set; }
        private Traverse traverse;
        private Traverse<WeaponCategoryValue> weapon_category;


        public LocationHardpointHelper(MechLabHardpointElement element)

        {
            Element = element;
            traverse = new Traverse(element);

            weapon_category = traverse.Field<WeaponCategoryValue>("currentWeaponCategoryValue");
            WeaponCategory = null;

            this.Text = traverse.Field<LocalizableText>("hardpointText").Value;
            this.TextColor = Text.GetComponent<UIColorRefTracker>();
            this.Icon = traverse.Field<SVGImage>("hardpointIcon").Value;
            this.IconColor = Icon.GetComponent<UIColorRefTracker>();
            this.Canvas = traverse.Field<CanvasGroup>("thisCanvasGroup").Value;
            this.Tooltip = element.GetComponent<HBSTooltip>();
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
                uicolor = HPInfo.WeaponCategory.GetUIColor();

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
            go.SetActive(false);
        }

        public override void Show()
        {
            go.SetActive(true);
        }
    }

    public class MechlabHardpointHelper : HardpointHelper
    {
        private GameObject go;
        public MechlabHardpointHelper(GameObject hpgo, HardpointInfo hpinfo)
        {
            go = hpgo;

            HPInfo = hpinfo;
            if (hpinfo.OverrideColor)
            {
                uicolor = UIColor.Custom;
                color = hpinfo.HPColor;
            }
            else
                uicolor = hpinfo.WeaponCategory.GetUIColor();

            Text = hpgo.GetComponentInChildren<LocalizableText>();
            Icon = hpgo.GetComponentInChildren<SVGImage>();

            TextColor = Text.GetComponent<UIColorRefTracker>();
            IconColor = Icon.GetComponent<UIColorRefTracker>();
            Tooltip = hpgo.GetComponent<HBSTooltip>();

            init(hpinfo.WeaponCategory.GetIcon(), hpinfo.TooltipCaption, hpinfo.Description);

        }

        public override void Hide()
        {
            go.SetActive(false);
        }

        public override void Show()
        {
            go.SetActive(true);
        }
    }
}