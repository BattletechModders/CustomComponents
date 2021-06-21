using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    public class LocationHelper
    {
        private Traverse main = null;

        private Traverse maxSlots = null, usedSlots = null;
        private Traverse location_name;

        public MechLabLocationWidget widget { get; private set; }
        public ChassisLocations Location => widget?.loadout?.Location ?? ChassisLocations.None;

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

        public List<HPUsage> HardpointsUsage { get; private set; }

        public LocationHardpointHelper[] HardpointWidgets { get; private set; }

        public void UpdateHardpointUsage()
        {
            if (MechLabHelper.CurrentMechLab?.ActiveMech == null)
            {
                HardpointsUsage = null;
                return;
            }

            HardpointsUsage = mechLab.activeMechDef.GetAllHardpoints(Location, mechLab.activeMechDef.Inventory.ToInvItems());
            //foreach (var hpUsage in HardpointsUsage)
            //{
            //    hpUsage.Used = 0;
            //}

            foreach (var item in LocalInventory
                .Select(i => i.ComponentRef.GetComponent<UseHardpointCustom>())
                .Where(i => i != null && !i.WeaponCategory.Is_NotSet))
            {
                HPUsage first = null;
                bool found = false;

                for (int i = 0; i < HardpointsUsage.Count; i++)
                {
                    var hp = HardpointsUsage[i];

                    if (!hp.hpInfo.CompatibleID.Contains(item.WeaponCategory.ID))
                        continue;
                    if (hp.Used < hp.Total)
                    {
                        found = true;
                        hp.Used += 1;
                    }

                    first ??= hp;
                }

                if (!found)
                    if (first == null)
                        HardpointsUsage.Add(new HPUsage(item.hpInfo, 0, -1));
                    else
                        first.Used += 1;
            }
        }

        #region old-hardpoints

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

        #endregion
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
            try
            {
                if (HardpointWidgets == null || HardpointWidgets.Length != 4)
                    return;

                //Control.Log($"{Location} - refresh hardpoints");

                int active_hp = 0;
                for (int i = 0; i < 4; i++)
                {
                    var widget = HardpointWidgets[i];

                    if (HardpointsUsage == null)
                    {
                        Control.LogError($"- {Location} widget #{i} not exist, skip");
                        widget.Hide();
                        continue;
                    }

                    while (active_hp < HardpointsUsage.Count && !HardpointsUsage[active_hp].hpInfo.Visible)
                        active_hp += 1;

                    if (active_hp < HardpointsUsage.Count)
                    {
                        var hp = HardpointsUsage[active_hp];

                        if (widget == null)
                        {
                            //Control.LogError($"- {i} widget null");
                            break;
                        }

                        if (widget.WeaponCategory == null || widget.WeaponCategory.ID != HardpointsUsage[active_hp].hpInfo.WeaponCategory.ID)
                            widget.Init(hp.hpInfo);
                        //Control.Log($"- {i} set to {hp.Used}/{hp.Total}");

                        widget.SetText(hp.Used, hp.Total);
                    }
                    else
                        widget.Hide();

                    active_hp += 1;
                }
            }
            catch (Exception e)
            {
                Control.LogError(e);
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
                .Select(i => new LocationHardpointHelper(i))
                .ToArray();

            UpdateHardpointUsage();
            RefreshHardpoints();
        }

    }
}