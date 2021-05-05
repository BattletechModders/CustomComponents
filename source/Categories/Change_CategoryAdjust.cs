using System.Collections.Generic;
using System.Linq;

namespace CustomComponents.Changes
{
    public class Change_CategoryAdjust : IChange_Adjust
    {
        public string CategoryID { get; private set; }
        public string ChangeID => "Category_" + CategoryID;
        public bool Initial { get; set; }

        public Change_CategoryAdjust(string categoryId)
        {
            CategoryID = categoryId;
        }


        public void AdjustChange(InventoryOperationState state)
        {
            var mech = state.Mech;
            var record = DefaultsDatabase.Instance[mech];
            if (record == null || mech == null)
                return;

            if (record.Multi != null)
            {
                Control.LogDebug(DType.InventoryOperaions, "--- DoMultiChange");

                DefaultFixer.Instance.DoMultiChange(state);
            }

            Control.LogDebug(DType.InventoryOperaions, "--- DoDefaultsChange");
            DefaultFixer.Instance.DoDefaultsChange(state, CategoryID);
        }

        public override string ToString()
        {
            return "Change_CategoryAdjust_" + CategoryID;
        }
    }
}