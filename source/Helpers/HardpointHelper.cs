using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using SVGImporter;
using UnityEngine;

namespace CustomComponents
{
    public class HardpointHelper
    {
        private Traverse traverse;
        private Traverse<WeaponCategoryValue> weapon_category;

        public WeaponCategoryValue WeaponCategory
        {
            get => weapon_category.Value;
            set => weapon_category.Value = value;
        }


        public MechLabHardpointElement Element { get; private set; }
        public LocalizableText Text { get; private set; }

        public SVGImage Icon { get; private set; }
        public CanvasGroup Canvas { get; private set; }

        public UIColorRefTracker TextColor { get; private set; }
        public UIColorRefTracker IconColor { get; private set; }

        public HardpointHelper(MechLabHardpointElement element)
        {
            Element = element;
            traverse = new Traverse(element);
            weapon_category = traverse.Field<WeaponCategoryValue>("currentWeaponCategoryValue");

            Text = traverse.Field<LocalizableText>("hardpointText").Value;
            TextColor = Text.GetComponent<UIColorRefTracker>();
            Icon = traverse.Field<SVGImage>("hardpointIcon").Value;
            IconColor = Icon.GetComponent<UIColorRefTracker>();
            Canvas = traverse.Field<CanvasGroup>("thisCanvasGroup").Value;
        }

        public void Hide()
        {
            Canvas.alpha = 0f;
        }

        public void SetData(HardpointInfo wc, string text)
        {
            if (wc?.WeaponCategory == null || wc.WeaponCategory.Is_NotSet || !wc.Visible)
            {
                Hide();
                return;
            }

            Canvas.alpha = 1f;
            Icon.vectorGraphics = wc.WeaponCategory.GetIcon();
            Text.SetText(text);

            if (Control.Settings.ColorHardpointsIcon)
            {
                IconColor.SetUIColor(wc.WeaponCategory.GetUIColor());
            }

            if (Control.Settings.ColorHardpointsText)
            {
                TextColor.SetUIColor(wc.WeaponCategory.GetUIColor());
            }

        }
    }
}