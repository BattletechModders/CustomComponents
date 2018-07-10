using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent, IAfterLoad, IMechLabFilter
    {
        private List<string> flags;

        [JsonIgnore]
        public bool CannotRemove { get; private set; }

        [JsonIgnore]
        public bool AutoRepair { get; private set; }

        [JsonIgnore]
        public bool HideFromInventory { get; private set; }

        [JsonIgnore]
        public bool NotSalvagable { get; private set; }

        [JsonIgnore]
        public bool Default => CannotRemove && AutoRepair && HideFromInventory && NotSalvagable;

        public bool CheckFilter(MechLabPanel panel)
        {
            return !HideFromInventory;
        }

        public bool IsSet(string value)
        {
            return flags.Contains(value);
        }

        public virtual void OnLoaded()
        {
            CannotRemove = false;
            AutoRepair = false;
            HideFromInventory = false;
            NotSalvagable = false;

            if (flags == null)
            {
                flags = new List<string>();
                return;
            }

            var new_flags = new List<string>();
            foreach (var flag in flags)
            {
                var f = flag.ToLower();
                new_flags.Add(f);
                switch (f)
                {
                    case "default":
                        CannotRemove = true;
                        AutoRepair = true;
                        HideFromInventory = true;
                        NotSalvagable = true;
                        new_flags.Add("autorepair");
                        new_flags.Add("no_remove");
                        new_flags.Add("hide");
                        new_flags.Add("no_salvage");
                        break;
                    case "autorepair":
                        AutoRepair = true;
                        break;
                    case "no_remove":
                        CannotRemove = true;
                        break;
                    case "hide":
                        HideFromInventory = true;
                        break;
                    case "no_salvage":
                        NotSalvagable = true;
                        break;
                }
            }

            flags = new_flags.Distinct().ToList();
        }
    }
}
