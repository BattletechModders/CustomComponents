using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent, IMechLabFilter, IMechValidate, IIsDestroyed
    {
        public List<string> flags;
        public string ErrorCannotRemove { get; set; }
        public string ErrorItemBroken { get; set; }
        public string ErrorItemDestroyed { get; set; }
        public string ErrorInvalid { get; set; }


        public bool CheckFilter(MechLabPanel panel)
        {
            return !HideFromInventory;
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
