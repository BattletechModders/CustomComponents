using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public interface IValidateAdd
    {
        bool ValidateAdd(MechLabLocationWidget widget, bool current_result, ref string errorMessage,
            MechLabPanel mechlab);
    }

    public interface IMechValidate
    {
        void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef);
    }
}