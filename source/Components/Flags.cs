using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent, IAfterLoad, IMechLabFilter, IOnItemGrab, IMechValidate
    {
        public List<string> flags;

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

        [JsonIgnore]
        public bool NotBroken { get; private set; }
        [JsonIgnore]
        public bool NotDestroyed { get; private set; }
        [JsonIgnore]
        public bool Invalid { get; private set; }


        public bool CheckFilter(MechLabPanel panel)
        {
            return !HideFromInventory;
        }

        public bool IsSet(string value)
        {
            return flags.Contains(value);
        }

        public bool OnItemGrab(IMechLabDraggableItem item, MechLabPanel ___mechLab, out string error)
        {
            error = null;
            if(CannotRemove)
            {
                error = "Cannot remove vital component";
                return false;
            }
            return true;
        }

        public virtual void OnLoaded(Dictionary<string, object> values)
        {
            CannotRemove = false;
            AutoRepair = false;
            HideFromInventory = false;
            NotSalvagable = false;
            NotBroken = false;
            NotDestroyed = false;
            Invalid = false;

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
                    case "not_broken":
                        NotBroken = true;
                        break;
                    case "not_destroyed":
                        NotDestroyed = true;
                        break;
                    case "invalid":
                        Invalid = true;
                        break;
                }
            }

            flags = new_flags.Distinct().ToList();
        }

        public void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if(Invalid)
                errors[MechValidationType.InvalidInventorySlots].Add($"{Def.Description.Name} is placeholder, remove it");

            if (componentRef.DamageLevel == ComponentDamageLevel.Destroyed && (NotDestroyed || NotBroken))
            {
                errors[MechValidationType.StructureDestroyed].Add($"{Def.Description.Name} is destroyed, Replace it");
            }

            if (componentRef.DamageLevel == ComponentDamageLevel.Penalized && NotBroken)
            {
                errors[MechValidationType.StructureDestroyed].Add($"{Def.Description.Name} is damaged, Repair it");
            }
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (Invalid)
                return false;

            if (NotDestroyed && componentRef.DamageLevel == ComponentDamageLevel.Destroyed)
                return false;

            if (NotBroken && (componentRef.DamageLevel == ComponentDamageLevel.Destroyed ||
                              componentRef.DamageLevel == ComponentDamageLevel.Penalized))
                return false;

            return true;
        }
    }
}
