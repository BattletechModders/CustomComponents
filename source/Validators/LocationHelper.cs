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
        public MechLabPanel mechLab { get
            {
                return widget.parentDropTarget as MechLabPanel;
            }
        }
        public Traverse cb_slots, ce_slots, cm_slots, cs_slots;

        public int mb_slots = -1, me_slots = -1, mm_slots = -1, ms_slots = -1;

        private List<MechLabItemSlotElement> inventory;


        public int currentBallisticCount
        {
            get
            {
                if (cb_slots == null)
                    cb_slots = main.Field("currentBallisticCount");
                return cb_slots.GetValue<int>();
            }
        }

        public int currentEnergyCount
        {
            get
            {
                if (ce_slots == null)
                    ce_slots = main.Field("currentEnergyCount");
                return ce_slots.GetValue<int>();
            }
        }

        public int currentMissileCount
        {
            get
            {
                if (cm_slots == null)
                    cm_slots = main.Field("currentMissileCount");
                return cm_slots.GetValue<int>();
            }
        }

        public int currentSmallCount
        {
            get
            {
                if (cs_slots == null)
                    cs_slots = main.Field("currentSmallCount");
                return cs_slots.GetValue<int>();
            }
        }

        public int totalBallisticHardpoints
        {
            get
            {
                if (mb_slots < 0)
                    mb_slots = main.Field("totalBallisticHardpoints").GetValue<int>();
                return mb_slots;
            }

        }

        public int totalEnergyHardpoints
        {
            get
            {
                if (me_slots < 0)
                    me_slots = main.Field("totalEnergyHardpoints").GetValue<int>();
                return me_slots;
            }

        }

        public int totalMissileHardpoints
        {
            get
            {
                if (mm_slots < 0)
                    mm_slots = main.Field("totalMissileHardpoints").GetValue<int>();
                return mm_slots;
            }

        }

        public int totalSmallHardpoints
        {
            get
            {
                if (ms_slots < 0)
                    ms_slots = main.Field("totalSmallHardpoints").GetValue<int>();
                return ms_slots;
            }

        }

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

        public LocationHelper(MechLabLocationWidget widget)
        {
            this.widget = widget;
            main = Traverse.Create(widget);

        }

    }
}