using System.Collections.Generic;
using System.Linq;

namespace CustomComponents.Changes
{
    public class Change_CategoryAdjust : IChange_Adjust
    {
        public string CategoryID { get; private set; }
        public string ChangeID => "Category_" + CategoryID;

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
                DefaultFixer.Instance.DoMultiChange(state);

            DefaultFixer.Instance.DoDefaultsChange(state, CategoryID);
        }
    }
}