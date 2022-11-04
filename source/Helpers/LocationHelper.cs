using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class LocationHelper
    {
        public MechLabLocationWidget widget { get; private set; }
        public ChassisLocations Location => widget?.loadout?.Location ?? ChassisLocations.None;

        public List<HPUsage> HardpointsUsage { get; private set; }

        public LocationHardpointHelper[] HardpointWidgets { get; }

        public void UpdateHardpointUsage()
        {
            if (MechLabHelper.CurrentMechLab?.ActiveMech == null)
            {
                HardpointsUsage = null;
                return;
            }
            HardpointsUsage = MechLabHelper.CurrentMechLab.ActiveMech.GetHardpointUsage(Location);

        }

        public List<MechLabItemSlotElement> LocalInventory => widget.localInventory;

        public string LocationName => widget.locationName.text;

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
            HardpointWidgets = widget.hardpoints
                .Select(i => new LocationHardpointHelper(i))
                .ToArray();

            UpdateHardpointUsage();
            RefreshHardpoints();
        }

    }
}