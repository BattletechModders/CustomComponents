using System;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using SVGImporter;
using UnityEngine;
using UIColor = BattleTech.UI.UIColor;

namespace CustomComponents
{

    public abstract class HardpointHelper
    {
        public HardpointInfo HPInfo { get; protected set; }

        public virtual void InitJJ(SVGAsset JJIcon)
        {
            HPInfo = null;

            color = Color.white;
            init(JJIcon);
        }

        protected void init(SVGAsset image)
        {
            Icon.vectorGraphics = image;
            Text.SetText("-");

            SetIconColor();
            SetTextColor();
            Show();
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

        public abstract void Hide();
        public abstract void Show();


        protected UIColor uicolor;
        protected Color color;


        public LocalizableText Text { get; protected set; }
        public SVGImage Icon { get; protected set; }
        public UIColorRefTracker TextColor { get; protected set; }
        public UIColorRefTracker IconColor { get; protected set; }

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

            init(HPInfo.WeaponCategory.GetIcon());
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


            init(Icon.vectorGraphics);
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


            init(icon.vectorGraphics);
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
            
            init(hpinfo.WeaponCategory.GetIcon());

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