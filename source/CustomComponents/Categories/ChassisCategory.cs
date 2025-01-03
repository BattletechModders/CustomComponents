﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents;

[CustomComponent("ChassisCategory", AllowArray = true)]
public class ChassisCategory : SimpleCustomChassis, IOnLoaded
{
    public class _record
    {
        public ChassisLocations Location { get; set; } = ChassisLocations.All;
        public int Min { get; set; } = 0;
        public int Max { get; set; } = -1;

        public override bool Equals(object obj)
        {
            var r = obj as _record;
            if (r == null)
            {
                return false;
            }

            return Location == r.Location;
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode();
        }
    }

    public string CategoryID { get; set; }
    private _record[] Limits { get; set; }

    [JsonIgnore]
    public Dictionary<ChassisLocations, CategoryLimit> LocationLimits { get; set; }

    public void OnLoaded()
    {
        var desc = CategoryController.Shared.GetCategory(CategoryID);

        if (Limits == null || Limits.Length == 0)
        {
            LocationLimits = new();
        }
        else
        {
            LocationLimits = Limits.Distinct().ToDictionary(i => i.Location, i => new CategoryLimit(i.Min, i.Max, desc?.ReplaceDefaultsFirst ?? true));
        }
    }
}