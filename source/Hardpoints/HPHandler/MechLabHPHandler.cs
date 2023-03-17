using System.Linq;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents;

public class MechLabHPHandler : HPHandler
{

    private Transform make_samlpe(Transform parent)
    {
        var sample = parent.GetChild(0);

        var lc = sample.gameObject.AddComponent<LayoutElement>();
        lc.minHeight = 30;
        lc.minWidth = 70;
        var rect = sample.GetChild(0).GetComponent<RectTransform>();
        rect.sizeDelta = new(42f, rect.sizeDelta.y);
        var text = sample.GetChild(0).GetComponent<LocalizableText>();
        text.alignment = TextAlignmentOptions.Midline;
        var l = sample.GetChild(1).gameObject.AddComponent<LayoutElement>();
        //l.minHeight = 26;
        l.minWidth = 24;
        //l.preferredHeight = 26;
        //l.preferredWidth = 26;
        var image = sample.gameObject.GetComponent<Image>();
        image.color = Control.Settings.GetHardpointBackDefaultColor();
        var hlg = sample.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.padding = new(2, 2, 2, 2);


        return sample;
    }

    public void Init(MechLabPanel panel)
    {
        var top_layout = panel.transform
            .Find("Representation/OBJGROUP_LEFT/OBJ_meta/OBJ_status");

        var hp_layout = top_layout.Find("layout_hardpoints");
        var horizontal = hp_layout.GetComponent<HorizontalLayoutGroup>();
        DestroyImmediate(horizontal);
        var jj_layout = top_layout.Find("layout_jumpjets");




        var vertical = hp_layout.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.childControlHeight = true;
        vertical.childControlWidth = true;
        vertical.padding = new(5, 5, 5, 5);
        vertical.spacing = 0;
        vertical.childAlignment = TextAnchor.MiddleCenter;
        vertical.enabled = true;

        var fitter = hp_layout.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;

        var transform = vertical.GetComponent<RectTransform>();
        transform.anchoredPosition = new(-80, transform.anchoredPosition.y);

        var jj = jj_layout.GetChild(0);
        var sample = make_samlpe(hp_layout);
        foreach (Transform child in hp_layout)
        {
            if (child != sample)
                Destroy(child.gameObject);
        }

        var jjgo = Instantiate(sample.gameObject);

        jjhardpoint = new(jjgo, jj);
        hardpoints = new();

        jjgo.transform.SetParent(hp_layout);
        foreach (var hpinfo in HardpointController.Instance.HardpointsList.Where(i => i.Visible))
        {
            var hpgo = Instantiate(sample.gameObject);
            hardpoints[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(hpgo, hpinfo);
            hpgo.transform.SetParent(hp_layout);
        }

        Destroy(sample.gameObject);
        Destroy(jj_layout.gameObject);

        var button = top_layout.Find("OBJ_stockBttn");
        button.SetParent(hp_layout);
        var l = button.gameObject.AddComponent<LayoutElement>();
        l.minHeight = 50;
    }
}