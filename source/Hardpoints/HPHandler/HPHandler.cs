using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomComponents
{
    public abstract class HPHandler : MonoBehaviour
    {
        protected Dictionary<int, HardpointHelper> hardpoints;
        protected JJHardpointHeler jjhardpoint;


        public void SetJJ(int count, int max)
        {
            jjhardpoint?.SetText(count, max);
            jjhardpoint?.Show();
        }

        public void SetJJ(int max)
        {
            if (jjhardpoint == null)
                return;

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

        public void SetData(List<HPUsage> usage)
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

        public void SetDataTotal(List<HPUsage> usage)
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