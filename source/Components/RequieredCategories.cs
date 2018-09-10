using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("RequiredCategories")]
    public class RequiredCategories : SimpleCustomComponent, IMechValidate
    {
        public string[] Categories { get; set; }
        public string ErrorMessage { get; set; }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef,
            MechComponentRef componentRef)
        {
            if (Categories == null || Categories.Length == 0)
                return;

            foreach (var category in Categories)
            {
                if (mechDef.Inventory.Any(i => i.IsCategory(category)))
                    return;
            }

            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.IsNullOrEmpty(ErrorMessage) ?
                $"{Def.Description.Name} requires additional modules to work" :
                ErrorMessage));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (Categories == null || Categories.Length == 0)
                return true;
            foreach (var category in Categories)
            {
                if (mechDef.Inventory.Any(i => i.IsCategory(category)))
                    return true;
            }
            return false;
        }
    }
}