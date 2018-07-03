using System.Data;
using BattleTech;

namespace CustomComponents
{
    public class BTValidateState
    {
        public enum ErrorType { None, Size, JumpJets, Hardpoints, LocationDestroyed, WrongLocation }

        public ErrorType Error; 
        public int RepaceIndex;
        public MechComponentDef Replacement;
    }
}