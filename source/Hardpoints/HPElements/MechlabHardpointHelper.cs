using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents
{
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
            BackImage = hpgo.GetComponent<Image>();

            TextColor = Text.GetComponent<UIColorRefTracker>();
            IconColor = Icon.GetComponent<UIColorRefTracker>();
            Tooltip = hpgo.GetComponent<HBSTooltip>();

            init(hpinfo.WeaponCategory.GetIcon(), hpinfo.TooltipCaption, hpinfo.Description);

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