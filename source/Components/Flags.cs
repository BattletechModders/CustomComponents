using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent, IAfterLoad, IMechLabFilter, IMechValidate, IIsDestroyed
    {
        public List<string> flags;

        [JsonIgnore] private bool _noremove;
        [JsonIgnore] private bool _arepair;
        [JsonIgnore] private bool _hideinv;
        [JsonIgnore] private bool _nosalvag;

        [JsonIgnore]
        public bool CannotRemove
        {
            get => _noremove || Def.IsDefault();
            private set { _noremove = value; }
        }

        [JsonIgnore]
        public bool AutoRepair
        {
            get => _arepair || Def.IsDefault();
            private set { _arepair = value; }
        }


        [JsonIgnore]
        public bool HideFromInventory
        {
            get => _hideinv || Def.IsDefault();
            private set { _hideinv = value; }
        }


        [JsonIgnore]
        public bool NotSalvagable
        {
            get => _nosalvag || Def.IsDefault();
            private set { _nosalvag = value; }
        }


        [JsonIgnore] public bool Default => Def.IsDefault();

        [JsonIgnore]
        public bool NotBroken
        {
            get;
            private set;
        }
        [JsonIgnore]
        public bool NotDestroyed { get; private set; }
        [JsonIgnore]
        public bool Invalid { get; private set; }
        [JsonIgnore]
        public bool DontShowMessage { get; private set; }
        [JsonIgnore]
        public bool IsVital { get; private set; }


        public string ErrorCannotRemove { get; set; }
        public string ErrorItemBroken { get; set; }
        public string ErrorItemDestroyed { get; set; }
        public string ErrorInvalid { get; set; }


        public bool CheckFilter(MechLabPanel panel)
        {
            return !HideFromInventory;
        }

        public bool IsSet(string value)
        {
            return flags.Contains(value);
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
            DontShowMessage = false;
            IsVital = false;

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
                        DefaultHelper.SetDefault(Def.Description.Id);
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
                    case "hide_remove_message":
                        DontShowMessage = true;
                        break;
                    case "vital":
                        IsVital = true;
                        break;
                }
            }

            flags = new_flags.Distinct().ToList();
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (Invalid)
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                    string.Format(string.IsNullOrEmpty(ErrorInvalid) ? "{0} is placeholder, remove it" : ErrorInvalid, Def.Description.Name)));

            if (componentRef.DamageLevel == ComponentDamageLevel.Destroyed && (NotDestroyed || NotBroken))
            {
                errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                    string.Format(string.IsNullOrEmpty(ErrorItemBroken) ? "{0} is destroyed, replace it" : ErrorItemDestroyed, Def.Description.Name)));
            }

            if (componentRef.DamageLevel == ComponentDamageLevel.Penalized && NotBroken)
            {
                errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                    string.Format(string.IsNullOrEmpty(ErrorItemBroken) ? "{0} is damaged, repair it" : ErrorItemBroken, Def.Description.Name)));
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

        public override string ToString()
        {
            return flags.Aggregate("Flags: [", (current, flag) => current + flag + " ") + "]";
        }

        public bool IsMechDestroyed(MechComponentRef component, MechDef mech)
        {
            return IsVital && component.DamageLevel == ComponentDamageLevel.Destroyed;
        }
    }
}
