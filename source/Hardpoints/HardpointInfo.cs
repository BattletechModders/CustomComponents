using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using fastJSON;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace CustomComponents
{
    public class HardpointInfo
    {
        public string ID { get; set; }
        public bool Visible { get; set; }
        private string [] Compatible { get; set; }
        public bool AllowOnWeapon { get; set; } = true;

        public bool AllowOmni { get; set; } = true;

        public bool OverrideColor { get; set; } = false;
        public Color HPColor { get; set; } = UnityEngine.Color.white;

        [JsonIgnore] public HashSet<int> CompatibleID { get; set; }
        [JsonIgnore] public WeaponCategoryValue WeaponCategory { get; set; }
        [JsonIgnore] public Color Color { get; set; }

        public bool Complete()
        {
            if (ID == null)
            {
                Control.LogError($"Empty WeaponCategory");
                return false;
            }

            WeaponCategory =
                WeaponCategoryEnumeration.GetWeaponCategoryByName(ID);

            if (WeaponCategory == null || WeaponCategory.Is_NotSet)
            {
                Control.LogError($"Unknown WeaponCategory {ID}");
                return false;
            }

            if (Compatible == null || Compatible.Length == 0)
            {
                Compatible = new[] { ID };
            }
            else
            {
                if (!Compatible.Contains(ID))
                    Compatible.Add(ID);
            }

            CompatibleID = Compatible
                .Distinct()
                .Select(i => WeaponCategoryEnumeration.GetWeaponCategoryByName(i))
                .Where(i => i != null)
                .Select(i => i.ID)
                .ToHashSet();

            return true;
        }
    }
}