using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("RequiredID")]
    public class RequiredID : SimpleCustomComponent, IMechValidate
    {
        public string[] ComponentDefId { get; set; }
        public string ErrorMessage { get; set; } 

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef,
            MechComponentRef componentRef)
        {
            if (ComponentDefId == null || ComponentDefId.Length == 0)
                return;
            foreach (var cref in mechDef.Inventory)
            {
                if (ComponentDefId.Contains(cref.ComponentDefID))
                    return;
            }
            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.IsNullOrEmpty(ErrorMessage) ? $"{Def.Description.Name} missed required components" : ErrorMessage));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (ComponentDefId == null || ComponentDefId.Length == 0)
                return true;
            foreach (var cref in mechDef.Inventory)
            {
                if (ComponentDefId.Contains(cref.ComponentDefID))
                    return true;
            }
            return false;
        }
    }
}