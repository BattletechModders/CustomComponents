﻿using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents;

public class DefaultsInfoRecord
{
    public ChassisLocations Location { get; set; } = ChassisLocations.None;
    public string DefID { get; set; }
    public ComponentType Type { get; set; }

    public override string ToString()
    {
        return $"[{DefID}({Type}) => {Location}]";
    }
}


public class UnitTypeDefaultsRecord
{
    public string UnitType { get; set; }
    public DefaultsInfoRecord[] Defaults { get; set; }

    public override string ToString()
    {
        var result = "\n- " + UnitType;
        if(Defaults != null && Defaults.Length > 0)
        {
            foreach (var defaultsInfoRecord in Defaults)
            {
                result += "\n-- " + defaultsInfoRecord;
            }
        }

        return result;
    }
}

public class DefaultsInfo
{
    public string CategoryID { get; set; }
    public UnitTypeDefaultsRecord[] UnitTypes { get; set; }
    public DefaultsInfoRecord[] Defaults { get; set; }

    public override string ToString()
    {
        var result = "Defaults for " + CategoryID;
        if (Defaults != null && Defaults.Length > 0)
        {
            result += "\n- Defaults";
            foreach (var defaultsInfoRecord in Defaults)
            {
                result += "\n-- " + defaultsInfoRecord;
            }
        }

        if (UnitTypes != null && UnitTypes.Length > 0)
        {
            foreach (var record in UnitTypes)
            {
                result += record.ToString();
            }
        }

        return result;
    }

    public void Complete()
    {
        if (Defaults == null && UnitTypes != null)
        {
            var item = UnitTypes.FirstOrDefault(i => i.UnitType == "*");
            if (item != null)
            {
                Defaults = item.Defaults;
            }
        }
    }

    public DefaultsInfoRecord[] GetDefault(HashSet<string> unit_types)
    {
        try
        {
            if (unit_types == null || UnitTypes == null || UnitTypes.Length == 0)
            {
                return Defaults;
            }

            foreach (var record in UnitTypes)
            {
                if (unit_types.Contains(record.UnitType))
                {
                    return record.Defaults;
                }
            }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(Defaults[0].DefID, e);
        }

        return Defaults;
    }
}