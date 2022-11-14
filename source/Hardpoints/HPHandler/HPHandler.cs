using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using UnityEngine;

namespace CustomComponents
{
    public abstract class HPHandler : MonoBehaviour
    {
        protected Dictionary<int, HardpointHelper> hardpoints;
        protected JJHardpointHeler jjhardpoint;

        // returns -1 if count can't be calculated
        // also set by MechEngineer
        public static Func<ChassisDef, int> GetJumpJetMaxByChassisDef { get; set; } = def => def?.MaxJumpjets ?? -1;
        public static Func<MechDef, (int, int)> GetJumpJetStatsByMechDef { get; set; } = def =>
        {
            var max = def?.Chassis?.MaxJumpjets ?? -1;
            var count = def?.Inventory.Count(i => i.ComponentDefType == ComponentType.JumpJet) ?? -1;
            return (count, max);
        };

        internal void SetJJ(MechDef mechDef)
        {
            var (count, max) = GetJumpJetStatsByMechDef(mechDef);
            jjhardpoint?.SetText(count, max);
            jjhardpoint?.Show();
        }

        internal void SetJJ(ChassisDef chassisDef)
        {
            if (jjhardpoint == null)
                return;

            var max = GetJumpJetMaxByChassisDef(chassisDef);
            if (max >= 0)
            {
                jjhardpoint.Show();
                jjhardpoint?.SetText(max);
            }
            else
            {
                jjhardpoint.Hide();
            }
        }

        internal void SetData(List<HPUsage> usage)
        {
            if (hardpoints == null)
                return;

            foreach (var widget in hardpoints)
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
        }

        internal void SetDataTotal(List<HPUsage> usage)
        {
            if (hardpoints == null)
                return;


            foreach (var widget in hardpoints)
            {
                var item = usage?.FirstOrDefault(i => i.hpInfo.WeaponCategory.ID == widget.Key);
                if (item != null)
                {
                    widget.Value.Show();
                    widget.Value.SetText(item.Total);
                }
                else
                    widget.Value.Hide();
            }
        }
    }
}