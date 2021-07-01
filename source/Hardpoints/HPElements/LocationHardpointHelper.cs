using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using SVGImporter;
using UnityEngine;

namespace CustomComponents
{
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
}