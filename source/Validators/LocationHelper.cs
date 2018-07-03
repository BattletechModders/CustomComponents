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

        public MechLabLocationWidget Location { get; private set; }
        private List<MechComponentRef> inventory;


        public List<MechComponentRef> LocalInventory
        {
            get
            {
                if (inventory == null)
                {
                    var inv = main.Field("localInventory");
                    inventory = inv.GetValue<List<MechLabItemSlotElement>>().Select(i => i.ComponentRef).ToList();
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

        public LocationHelper(MechLabLocationWidget location)
        {
            this.Location = location;
            main = Traverse.Create(location);

        }

    }
}