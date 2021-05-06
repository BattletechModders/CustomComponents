using CustomComponents.Changes;

namespace CustomComponents
{
    public class Change_WeaponAdjust : IChange_Adjust
    {
        public string ChangeID => "Weapon";
        public bool Initial { get; set; }
        public void AdjustChange(InventoryOperationState state)
        {

        }

    }
}