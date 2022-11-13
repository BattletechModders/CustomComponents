using System.Collections.Generic;
using System.Linq;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents
{
    public class UIModuleHPHandler : HPHandler
    {
        private Transform make_samlpe(Transform parent)
        {
            var sample = parent.GetChild(0);

            var lc = sample.gameObject.AddComponent<LayoutElement>();
            lc.minHeight = 30;
            lc.minWidth = 70;
            var rect = sample.GetChild(0).GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(42f, rect.sizeDelta.y);
            var text = sample.GetChild(0).GetComponent<LocalizableText>();
            text.gameObject.AddComponent<LayoutElement>();
            text.alignment = TextAlignmentOptions.Midline;
            var l = sample.GetChild(1).gameObject.AddComponent<LayoutElement>();
            //l.minHeight = 26;
            l.minWidth = 24;
            //l.preferredHeight = 26;
            //l.preferredWidth = 26;
            var image = sample.gameObject.AddComponent<Image>();
            image.color = Control.Settings.GetHardpointBackDefaultColor();
            var hlg = sample.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(0, 3, 3, 2);
            hlg.enabled = true;
            return sample;
        }

        public void Init(UIModule widget, GameObject hardpointtext, GameObject jjtext, Vector2 position)
        {
            var hp_layout = hardpointtext.transform.parent.parent;

            var horizontal = hp_layout.GetComponent<HorizontalLayoutGroup>();
            DestroyImmediate(horizontal);
            var jj_layout = jjtext.transform.parent.parent;



            var grid = hp_layout.gameObject.AddComponent<GridLayoutGroup>();
            //grid.childControlHeight = true;
            //vertical.childControlWidth = true;
            grid.padding = new RectOffset(3, 3, 3, 3);
            grid.spacing = new Vector2(1, 1);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(60, 32);
            var transform = hp_layout.GetComponent<RectTransform>();
            transform.anchoredPosition = position;
            transform.sizeDelta = new Vector2(295, 66);
            grid.enabled = true;

            var fitter = hp_layout.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;


            var jj = jj_layout.GetChild(0);
            var sample = make_samlpe(hp_layout);
            foreach (Transform child in hp_layout)
            {
                if (child != sample)
                    Destroy(child.gameObject);
            }

            var jjgo = Instantiate(sample.gameObject);

            jjhardpoint = new JJHardpointHeler(jjgo, jj);
            hardpoints = new Dictionary<int, HardpointHelper>();

            jjgo.transform.SetParent(hp_layout);
            foreach (var hpinfo in HardpointController.Instance.HardpointsList.Where(i => i.Visible))
            {
                var hpgo = Instantiate(sample.gameObject);
                hardpoints[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(hpgo, hpinfo);
                hpgo.transform.SetParent(hp_layout);
            }

            Destroy(sample.gameObject);
            Destroy(jj_layout.gameObject);
        }
    }
}