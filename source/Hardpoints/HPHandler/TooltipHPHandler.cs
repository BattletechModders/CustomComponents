using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents
{
    public class TooltipHPHandler : HPHandler
    {
        private Transform make_samlpe(Transform sample)
        {

            var lc = sample.gameObject.AddComponent<LayoutElement>();
            lc.minHeight = 35;
            lc.minWidth = 72;

            var rect = sample.GetChild(0).GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(42f, rect.sizeDelta.y);
            var text = sample.GetChild(0).GetComponent<LocalizableText>();
            var l = text.gameObject.AddComponent<LayoutElement>();
            l.minWidth = 38;
            text.alignment = TextAlignmentOptions.Midline;
            l = sample.GetChild(1).gameObject.AddComponent<LayoutElement>();
            //l.minHeight = 26;
            l.minWidth = 24;
            //l.preferredHeight = 26;
            //l.preferredWidth = 26;
            var image = sample.gameObject.AddComponent<Image>();
            image.color = Color.black;
            var hlg = sample.gameObject.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(3, 1, 1, 2);
            hlg.enabled = true;
            return sample;
        }

        public void Init(TooltipPrefab widget, GameObject hardpoint)
        {

            try
            {
                //Control.Log($"hardpoint name is {hardpoint.name}");
                var hp_layout = hardpoint.transform.parent;

                var horizontal = hp_layout.GetComponent<HorizontalLayoutGroup>();
                GameObject.DestroyImmediate(horizontal);

                var grid = hp_layout.gameObject.AddComponent<GridLayoutGroup>();
                //grid.childControlHeight = true;
                //vertical.childControlWidth = true;
                grid.padding = new RectOffset(3, 3, 3, 3);
                grid.spacing = new Vector2(1, 1);
                grid.childAlignment = TextAnchor.MiddleCenter;
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;
                grid.cellSize = new Vector2(73, 35);
                var transform = hp_layout.GetComponent<RectTransform>();
                //transform.anchoredPosition = position;
                //transform.sizeDelta = new Vector2(295, 66);
                grid.enabled = true;

                var fitter = hp_layout.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;


                var sample = make_samlpe(hardpoint.transform);
                foreach (Transform child in hp_layout)
                { 
                    child.gameObject.SetActive(false);
                }

                var jjgo = GameObject.Instantiate(sample.gameObject);

                jjhardpoint = new JJHardpointHeler(jjgo, sample);
                hardpoints = new Dictionary<int, HardpointHelper>();
                jjgo.transform.SetParent(hp_layout);
                jjgo.SetActive(true);
                foreach (var hpinfo in HardpointController.Instance.HardpointsList.Where(i => i.Visible))
                {
                    var hpgo = GameObject.Instantiate(sample.gameObject);
                    hardpoints[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(hpgo, hpinfo);
                    hpgo.transform.SetParent(hp_layout);
                    hpgo.SetActive(true);

                }
                sample.gameObject.SetActive(false);

                //GameObject.Destroy(sample.gameObject);
            }
            catch (Exception e)
            {
                Control.LogError(e);

            }
        }
    }
}