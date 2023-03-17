using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents;

public class MechLabHelper
{
    public static MechLabHelper CurrentMechLab { get; private set; }

    private HPHandler hardpoints;

    internal static void EnterMechLab(MechLabPanel mechlab)
    {
        if (Control.Settings.DEBUG_ShowMechUT)
        {
            var ut = mechlab.activeMechDef.GetUnitTypes();
            if (ut == null)
                Log.Main.Info?.Log($"Enter MechLab for {mechlab.activeMechDef.Description.Id}, UT:[ ]");
            else
                Log.Main.Info?.Log($"Enter MechLab for {mechlab.activeMechDef.Description.Id}, UT:[{ut.Join()}]");
        }

        CurrentMechLab = new(mechlab);
        CurrentMechLab.MakeLocations();

        CurrentMechLab.MakeHardpoints();
        CurrentMechLab.RefreshHardpoints();
    }

    private void MakeHardpoints()
    {
        try
        {
            hardpoints = MechLab.GetComponent<HPHandler>();
            if (hardpoints == null)
            {
                var mhp = MechLab.gameObject.AddComponent<MechLabHPHandler>();
                mhp.Init(MechLab);
                hardpoints = mhp;
            }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }

    //private void ReMakeHardpoints(Transform hpLayout)
    //{
    //    HardpointWidgets = new Dictionary<int, MechlabHardpointHelper>();
    //    JJWidget = new JJHardpointHeler(hpLayout.GetChild(0).gameObject);

    //    var list = HardpointController.Instance.HardpointsList.Where(i => i.Visible).ToArray();
    //    for (int i = 1; i < hpLayout.childCount - 1; i++)
    //    {
    //        var obj = hpLayout.GetChild(i).gameObject;

    //        if (i - 1 >= list.Length)
    //        {
    //            GameObject.Destroy(obj);
    //            Control.LogError($"-- Missed hardpoint element for {i}");
    //        }

    //        var hpinfo = list[i - 1];

    //        HardpointWidgets[hpinfo.WeaponCategory.ID] = new MechlabHardpointHelper(obj, hpinfo);
    //    }
    //}



    internal static void CloseMechLab()
    {
        CurrentMechLab = null;
    }

    public MechLabPanel MechLab { get; private set; }

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
    public bool InMechLab => CurrentMechLab != null;
    public bool InSimGame => MechLab.IsSimGame;

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

    public MechLabHelper(MechLabPanel mechLab) // !TODO PONE FIX IT
    {
        MechLab = mechLab;
    }

    private void MakeLocations()
    {
        LHelper_CT = new(MechLab.centerTorsoWidget);
        LHelper_HD = new(MechLab.headWidget);
        LHelper_RT = new(MechLab.rightTorsoWidget);
        LHelper_LT = new(MechLab.leftTorsoWidget);

        LHelper_LA = new(MechLab.leftArmWidget);
        LHelper_RA = new(MechLab.rightArmWidget);
        LHelper_LL = new(MechLab.leftLegWidget);
        LHelper_RL = new(MechLab.rightLegWidget);

        LHelper_SP = GetLocationHelperSP();

        all_helpers = new();

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
                        usage.Add(new(hpUsage));
                    else
                    {
                        item.Total += hpUsage.Total;
                        item.Used += hpUsage.Used;
                    }
                }
        }

        if (hardpoints != null)
        {
            hardpoints.SetData(usage);
            hardpoints.SetJJ(ActiveMech);
        }
    }
}