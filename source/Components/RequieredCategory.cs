using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("RequiredCategory")]
    public class RequieredCategory : SimpleCustomComponent, IMechValidate
    {
        public string CategoryID { get; set; }
        public string ErrorMessage { get; set; }

        public void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationLevel, MechDef mechDef,
            MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(CategoryID))
                return;
            var category = Control.GetCategory(CategoryID);
            if (mechDef.Inventory.Any(i => i.IsCategory(CategoryID)))
                return;
            errors[MechValidationType.InvalidInventorySlots].Add(string.IsNullOrEmpty(ErrorMessage) ?
                $"{Def.Description.Name} requшere {category.displayName} installed" :
                ErrorMessage);
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(CategoryID))
                return true;
            return mechDef.Inventory.Any(i => i.IsCategory(CategoryID));
        }
    }
}