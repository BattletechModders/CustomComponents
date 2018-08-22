using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("RequiredID")]
    public class RequiredID : SimpleCustomComponent, IMechValidate
    {
        public string ComponentDefId { get; set; }
        public string ErrorMessage { get; set; } 

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef,
            MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(ComponentDefId))
                return;
            foreach (var cref in mechDef.Inventory)
            {
                if (cref.ComponentDefID == ComponentDefId)
                    return;
            }
            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(string.IsNullOrEmpty(ErrorMessage) ? $"{Def.Description.Name} missed required components" : ErrorMessage));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            if (string.IsNullOrEmpty(ComponentDefId))
                return true;
            foreach (var cref in mechDef.Inventory)
            {
                if (cref.ComponentDefID == ComponentDefId)
                    return true;
            }
            return false;
        }
    }
}