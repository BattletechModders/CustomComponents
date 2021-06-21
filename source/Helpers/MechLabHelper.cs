using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Linq;
using BattleTech.UI.TMProWrapper;
using ErosionBrushPlugin;
using FluffyUnderware.DevTools.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomComponents
{
    public class MechLabHelper
    {
        private static MechLabHelper mechlab_instance;

        public static MechLabHelper CurrentMechLab
        {
            get { return mechlab_instance; }
        }



        internal static void EnterMechLab(MechLabPanel mechlab)
        {
            if (Control.Settings.DEBUG_ShowMechUT)
            {
                var ut = mechlab.activeMechDef.GetUnitTypes();
                if (ut == null)
                    Control.Log($"Enter MechLab for {mechlab.activeMechDef.Description.Id}, UT:[ ]");
                else
                    Control.Log($"Enter MechLab for {mechlab.activeMechDef.Description.Id}, UT:[{ut.Join()}]");
            }

            mechlab_instance = new MechLabHelper(mechlab);
            mechlab_instance.MakeLocations();

            mechlab_instance.MakeHardpoints();
            mechlab_instance.RefreshHardpoints();
        }

        public JJHardpointHeler JJWidget { get; private set; }

        public Dictionary<int, MechlabHardpointHelper> HardpointWidgets { get; private set; }

        private void MakeHardpoints()
        {
            try
            {
                var top_layout = MechLab.transform
                    .Find("Representation/OBJGROUP_LEFT/OBJ_meta/OBJ_status");

                if (top_layout == null)
                    return;

                

                var hp_layout = top_layout.Find("layout_hardpoints");
                if (hp_layout == null)
                    return;

                var horizontal = hp_layout.GetComponent<HorizontalLayoutGroup>();

                if (horizontal == null)
                {
                    ReMakeHardpoints(hp_layout);
                    return;
                }


                GameObject.DestroyImmediate(horizontal);
                var jj_layout = top_layout.Find("layout_jumpjets");
                if (jj_layout == null)
                    return;




                var vertical = hp_layout.gameObject.AddComponent<VerticalLayoutGroup>();
                vertical.childControlHeight = true;
                vertical.childControlWidth = true;
                vertical.padding = new RectOffset(5, 5, 5, 5);
                vertical.spacing = 1;
                vertical.childAlignment = TextAnchor.MiddleCenter;
                vertical.enabled = true;

                var fitter = hp_layout.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;

                var transform = vertical.GetComponent<RectTransform>();
                transform.anchoredPosition = new Vector2(-80, transform.anchoredPosition.y);

                var jj = jj_layout.GetChild(0);
                var sample = make_samlpe(hp_layout);
                foreach (Transform child in hp_layout)
                {
                    if (child != sample)
                        GameObject.Destroy(child.gameObject);
                }

                var jjgo = GameObject.Instantiate(sample.gameObject);

                JJWidget = new JJHardpointHeler(jjgo, jj);
                HardpointWidgets = new Dictionary<int, MechlabHardpointHelper>();
                jjgo.transform.SetParent(hp_layout);
                foreach (var hpinfo in HardpointController.Instance.HardpointsList.Where(i => i.Visible))
                {
                    var hpgo = GameObject.Instantiate(sample.gameObject);
                    HardpointWidgets[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(hpgo, hpinfo);
                    hpgo.transform.SetParent(hp_layout);
                }

                GameObject.Destroy(sample.gameObject);
                GameObject.Destroy(jj_layout.gameObject);

                var button = top_layout.Find("OBJ_stockBttn");
                button.SetParent(hp_layout);
                var l = button.gameObject.AddComponent<LayoutElement>();
                l.minHeight = 50;
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }

        }

        private void ReMakeHardpoints(Transform hpLayout)
        {
            HardpointWidgets = new Dictionary<int, MechlabHardpointHelper>();
            JJWidget = new JJHardpointHeler(hpLayout.GetChild(0).gameObject);

            var list = HardpointController.Instance.HardpointsList.Where(i => i.Visible).ToArray();
            for (int i = 1; i < hpLayout.childCount - 1; i++)
            {
                var obj = hpLayout.GetChild(i).gameObject;

                if (i - 1 >= list.Length)
                {
                    GameObject.Destroy(obj);
                    Control.LogError($"-- Missed hardpoint element for {i}");
                }

                var hpinfo = list[i - 1];

                HardpointWidgets[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(obj, hpinfo);
            }
        }

        private Transform make_samlpe(Transform parent)
        {
            var samlpe = parent.GetChild(0);

            var lc = samlpe.gameObject.AddComponent<LayoutElement>();
            lc.minHeight = 25;
            lc.minWidth = 65;
            var rect = samlpe.GetChild(0).GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(42f, rect.sizeDelta.y);
            var text = samlpe.GetChild(0).GetComponent<LocalizableText>();
            text.alignment = TextAlignmentOptions.Midline;
            var image = samlpe.gameObject.GetComponent<Image>();
            image.color = Color.black;
            var hlg = samlpe.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.padding = new RectOffset(2,2,0,0);
            

            return samlpe;
        }

        internal static void CloseMechLab()
        {
            mechlab_instance = null;
        }

        public MechLabPanel MechLab { get; private set; }


        private Traverse main;
        private Traverse drag_item;
        private Traverse<MechLabInventoryWidget> inventory;
        private Traverse<MechLabDismountWidget> bin;
        private List<LocationHelper> all_helpers;
        public LocationHelper LHelper_HD { get; private set; }
        public LocationHelper LHelper_CT { get; private set; }
        public LocationHelper LHelper_LT { get; private set; }
        public LocationHelper LHelper_RT { get; private set; }
        public LocationHelper LHelper_LA { get; private set; }
        public LocationHelper LHelper_RA { get; private set; }
        public LocationHelper LHelper_LL { get; private set; }
        public LocationHelper LHelper_RL { get; private set; }

        public LocationHelper LHelper_SP { get; private set; }

        public MechDef ActiveMech => MechLab.activeMechDef;
        public bool InMechLab => mechlab_instance != null;
        public bool InSimGame => MechLab.IsSimGame;
        public IEnumerable<MechLabLocationWidget> GetWidgets()
        {
            yield return MechLab.headWidget;
            yield return MechLab.leftArmWidget;
            yield return MechLab.leftTorsoWidget;
            yield return MechLab.centerTorsoWidget;
            yield return MechLab.rightTorsoWidget;
            yield return MechLab.rightArmWidget;
            yield return MechLab.leftLegWidget;
            yield return MechLab.rightLegWidget;
        }

        public IEnumerable<LocationHelper> GetLocationHelpers()
        {
            return all_helpers;
        }

        public IEnumerable<InvItem> FullInventory
        {
            get
            {
                return all_helpers.SelectMany(i => i.LocalInventory, (a, b) => new InvItem(b.ComponentRef, a.widget.loadout.Location));
            }
        }

        public MechLabLocationWidget GetLocationWidget(ChassisLocations location)
        {
            switch (location)
            {
                case ChassisLocations.Head:
                    return MechLab.headWidget;
                case ChassisLocations.LeftArm:
                    return MechLab.leftArmWidget;
                case ChassisLocations.LeftTorso:
                    return MechLab.leftTorsoWidget;
                case ChassisLocations.CenterTorso:
                    return MechLab.centerTorsoWidget;
                case ChassisLocations.RightTorso:
                    return MechLab.rightTorsoWidget;
                case ChassisLocations.RightArm:
                    return MechLab.rightArmWidget;
                case ChassisLocations.LeftLeg:
                    return MechLab.leftLegWidget;
                case ChassisLocations.RightLeg:
                    return MechLab.rightLegWidget;
            }

            return null;
        }

        public MechLabInventoryWidget InventoryWidget
        {
            get
            {
                if (inventory == null)
                    inventory = main.Field<MechLabInventoryWidget>("inventoryWidget");
                return inventory.Value;
            }
        }

        public MechLabDismountWidget DismountWidget
        {
            get
            {
                if (bin == null)
                    bin = main.Field<MechLabDismountWidget>("dismountWidget");
                return bin.Value;
            }
        }

        public MechLabHelper(MechLabPanel mechLab) // !TODO PONE FIX IT
        {
            MechLab = mechLab;
            main = Traverse.Create(mechLab);

        }

        private void MakeLocations()
        {
            LHelper_CT = new LocationHelper(MechLab.centerTorsoWidget);
            LHelper_HD = new LocationHelper(MechLab.headWidget);
            LHelper_RT = new LocationHelper(MechLab.rightTorsoWidget);
            LHelper_LT = new LocationHelper(MechLab.leftTorsoWidget);

            LHelper_LA = new LocationHelper(MechLab.leftArmWidget);
            LHelper_RA = new LocationHelper(MechLab.rightArmWidget);
            LHelper_LL = new LocationHelper(MechLab.leftLegWidget);
            LHelper_RL = new LocationHelper(MechLab.rightLegWidget);

            LHelper_SP = GetLocationHelperSP();

            all_helpers = new List<LocationHelper>();

            all_helpers.Add(LHelper_CT);
            all_helpers.Add(LHelper_HD);
            all_helpers.Add(LHelper_RT);
            all_helpers.Add(LHelper_LT);
            all_helpers.Add(LHelper_LA);
            all_helpers.Add(LHelper_RA);
            all_helpers.Add(LHelper_LL);
            all_helpers.Add(LHelper_RL);
            if (LHelper_SP != null)
                all_helpers.Add(LHelper_SP);
        }


        internal LocationHelper GetLocationHelper(ChassisLocations location)
        {
            switch (location)
            {
                case ChassisLocations.Head:
                    return LHelper_HD;
                case ChassisLocations.LeftArm:
                    return LHelper_LA;
                case ChassisLocations.LeftTorso:
                    return LHelper_LT;
                case ChassisLocations.CenterTorso:
                    return LHelper_CT;
                case ChassisLocations.RightTorso:
                    return LHelper_RT;
                case ChassisLocations.RightArm:
                    return LHelper_RA;
                case ChassisLocations.LeftLeg:
                    return LHelper_LL;
                case ChassisLocations.RightLeg:
                    return LHelper_RL;
            }

            return null;
        }

        private LocationHelper GetLocationHelperSP()
        {
            return null;
        }

        public void SetDragItem(MechLabItemSlotElement item)
        {
            if (drag_item == null)
                drag_item = main.Field("dragItem");

            drag_item.SetValue(item);
        }

        public void RefreshHardpoints()
        {
            var usage = new List<HPUsage>();

            foreach (var locationHelper in all_helpers)
            {
                locationHelper.UpdateHardpointUsage();
                locationHelper.RefreshHardpoints();

                if(locationHelper.HardpointsUsage != null)
                    foreach (var hpUsage in locationHelper.HardpointsUsage)
                    {
                        var item = usage.FirstOrDefault(i => i.hpInfo.WeaponCategory.ID == hpUsage.WeaponCategoryID);
                        if (item == null)
                            usage.Add(new HPUsage(hpUsage));
                        else
                        {
                            item.Total += hpUsage.Total;
                            item.Used += hpUsage.Used;
                        }
                    }
            }

            foreach (var widget in HardpointWidgets)
            {
                var item = usage.FirstOrDefault(i => i.hpInfo.WeaponCategory.ID == widget.Key);
                if (item != null)
                {
                    widget.Value.Show();
                    widget.Value.SetText(item.Used, item.Total);
                }
                else
                    widget.Value.Hide();
            }
            JJWidget.SetText(ActiveMech.GetJJCount(), ActiveMech.GetJJMax());
        }
    }
}