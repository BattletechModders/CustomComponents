using BattleTech;

namespace CustomComponents
{
    public class CategoryValidatorState
    {
        public CategoryError Error;
        public bool NotEnoughSlots;
        public MechComponentDef Replacement;
        public int ReplacementIndex;
        public CategoryDescriptor descriptor;
    }
}