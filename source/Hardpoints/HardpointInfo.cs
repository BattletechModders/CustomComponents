using System;
using System.Linq;
using BattleTech;
using fastJSON;
using FluffyUnderware.DevTools.Extensions;
using SVGImporter;
using UnityEngine;

namespace CustomComponents
{
    public class HardpointInfo
    {
        public string ID { get; set; }
        public string IconColor { get; set; }
        public bool Visible { get; set; }
        public string Short { get; set; }
        public string DisplayName { get; set; }
        public string[] Compatible { get; set; }
        public bool AcceptOmni { get; set; }

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

            if (Visible)
            {
                if (string.IsNullOrEmpty(Short))
                    Short = ID.Substring(0, 2);
                if (string.IsNullOrEmpty(DisplayName))
                    DisplayName = ID;
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

            Compatible = Compatible.Distinct().ToArray();

            return true;
        }
    }
}