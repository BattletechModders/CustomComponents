using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using SVGImporter;
using UnityEngine;

namespace CustomComponents
{
    public class LocationHelper
    {
        public class HPUsage
        {
            public int Total;
            public int Used;
            public HardpointInfo hpInfo;
        }

        private Traverse main = null;

        private Traverse maxSlots = null, usedSlots = null;
        private Traverse location_name;

        public MechLabLocationWidget widget { get; private set; }
        public MechLabPanel mechLab
        {
            get
            {
                return widget.parentDropTarget as MechLabPanel;
            }
        }
        public Traverse cb_slots, ce_slots, cm_slots, cs_slots;

        public int mb_slots = -1, me_slots = -1, mm_slots = -1, ms_slots = -1;

        private List<MechLabItemSlotElement> inventory;
        private List<HPUsage> base_hp;

        public IReadOnlyList<HardpointInfo> Hardpoints { get; private set; }

        public List<HPUsage> HardpointsUsage { get; private set; }

        public HardpointHelper[] HardpointWidgets { get; private set; }

        public void UpdateHardpointUsage()
        {
            if (base_hp == null)
            {
                base_hp = MechLabHelper.CurrentMechLab.ActiveMech.Chassis.GetLocationDef(widget.loadout.Location)
                    .Hardpoints
                    .GroupBy(i => i.WeaponMountValue)
                    .Select(i => new HPUsage()
                    {
                        hpInfo = HardpointController.Instance[i.Key],
                        Total = i.Count(),
                        Used = 0
                    })
                    .OrderBy(i => i.hpInfo.CompatibleID.Count)
                    .ToList();
            }

            HardpointsUsage = base_hp.Select(i => new HPUsage() { hpInfo = i.hpInfo, Total = i.Total, Used = 0}).ToList();


            HardpointsUsage.RemoveAll(a => a.Total <= 0);
            HardpointsUsage.Sort((a,b)=>a.hpInfo.CompatibleID.Count.CompareTo(b.hpInfo.CompatibleID.Count));
        }
        
        //public int currentBallisticCount
        //{
        //    get
        //    {
        //        if (cb_slots == null)
        //            cb_slots = main.Field("currentBallisticCount");
        //        return cb_slots.GetValue<int>();
        //    }
        //}

        //public int currentEnergyCount
        //{
        //    get
        //    {
        //        if (ce_slots == null)
        //            ce_slots = main.Field("currentEnergyCount");
        //        return ce_slots.GetValue<int>();
        //    }
        //}

        //public int currentMissileCount
        //{
        //    get
        //    {
        //        if (cm_slots == null)
        //            cm_slots = main.Field("currentMissileCount");
        //        return cm_slots.GetValue<int>();
        //    }
        //}

        //public int currentSmallCount
        //{
        //    get
        //    {
        //        if (cs_slots == null)
        //            cs_slots = main.Field("currentSmallCount");
        //        return cs_slots.GetValue<int>();
        //    }
        //}

        //public int totalBallisticHardpoints
        //{
        //    get
        //    {
        //        if (mb_slots < 0)
        //            mb_slots = main.Field("totalBallisticHardpoints").GetValue<int>();
        //        return mb_slots;
        //    }

        //}

        //public int totalEnergyHardpoints
        //{
        //    get
        //    {
        //        if (me_slots < 0)
        //            me_slots = main.Field("totalEnergyHardpoints").GetValue<int>();
        //        return me_slots;
        //    }

        //}

        //public int totalMissileHardpoints
        //{
        //    get
        //    {
        //        if (mm_slots < 0)
        //            mm_slots = main.Field("totalMissileHardpoints").GetValue<int>();
        //        return mm_slots;
        //    }

        //}

        //public int totalSmallHardpoints
        //{
        //    get
        //    {
        //        if (ms_slots < 0)
        //            ms_slots = main.Field("totalSmallHardpoints").GetValue<int>();
        //        return ms_slots;
        //    }

        //}

        public List<MechLabItemSlotElement> LocalInventory
        {
            get
            {
                if (inventory == null)
                {
                    var inv = main.Field("localInventory");
                    inventory = inv.GetValue<List<MechLabItemSlotElement>>();
                }

                return inventory;
            }
        }

        public int MaxSlots
        {
            get
            {
                if (maxSlots == null)
                    maxSlots = main.Field("maxSlots");
                return maxSlots.GetValue<int>();
            }
        }

        public int UsedSlots
        {
            get
            {
                if (usedSlots == null)
                    usedSlots = main.Field("usedSlots");
                return usedSlots.GetValue<int>();
            }
        }

        public string LocationName
        {
            get
            {
                if (location_name == null)
                    location_name = main.Field("locationName").Property("text");
                return location_name.GetValue<string>();
            }
        }

        public void RefreshHardpoints()
        {
            int active_hp = 0;
            for (int i = 0; i < 4; i++)
            {
                if (HardpointsUsage == null)
                    HardpointWidgets[i].Hide();

                while (active_hp < HardpointsUsage.Count && !HardpointsUsage[active_hp].hpInfo.Visible)
                    active_hp += 1;

                if (active_hp < HardpointsUsage.Count)
                    HardpointWidgets[i].SetData(HardpointsUsage[active_hp].hpInfo, $"{HardpointsUsage[active_hp].Used}/{HardpointsUsage[active_hp].Total}");
                else
                    HardpointWidgets[i].Hide();
            }
        }

        public LocationHelper(MechLabLocationWidget widget)
        {
            this.widget = widget;
            main = Traverse.Create(widget);
            var hplist = new List<HardpointInfo>();
            var mech = MechLabHelper.CurrentMechLab.ActiveMech;
            HardpointWidgets = main
                .Field<MechLabHardpointElement[]>("hardpoints")
                .Value
                .Select(i => new HardpointHelper(i))
                .ToArray();

            UpdateHardpointUsage();
            RefreshHardpoints();
        }

    }

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

            if (Control.Settings.ColorHardpoints)
            {
                IconColor.SetUIColor(wc.WeaponCategory.GetUIColor());
                TextColor.SetUIColor(wc.WeaponCategory.GetUIColor());
            }
        }
    }
}