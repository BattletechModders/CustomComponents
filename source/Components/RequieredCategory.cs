using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("RequiredCategory")]
    public class RequiredCategory : SimpleCustomComponent, IMechValidate
    {
        public string CategoryID { get; set; }
        public string ErrorMessage { get; set; }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef,
            MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(CategoryID))
                return;
            var category = Control.GetCategory(CategoryID);
            if (mechDef.Inventory.Any(i => i.IsCategory(CategoryID)))
                return;
            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.IsNullOrEmpty(ErrorMessage) ?
                $"{Def.Description.Name} requires {category.displayName} installed" :
                ErrorMessage));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(CategoryID))
                return true;
            return mechDef.Inventory.Any(i => i.IsCategory(CategoryID));
        }
    }
}