using System.Collections.Generic;
using System.Linq;

namespace CustomComponents.Changes
{
    public class CategoryDefaultsAdjust : IAdjustChange
    {
        public string CategoryID { get; private set; }
        public string ChangeID => "Category_" + CategoryID;

        public CategoryDefaultsAdjust(string categoryId)
        {
            CategoryID = categoryId;
        }

        public void DoChange(Queue<IChange> changes, List<InvItem> inventory)
        {
            var record = DefaultsDatabase.Instance[mech];
            if (record == null || mech == null)
                return false;

            if (record.Multi != null && record.Multi.UsedCategories.ContainsKey(CategoryID))
            {
                var mchanges = DefaultFixer.Instance.GetMultiChange(mech, inventory);
                if (mchanges != null)
                {
                    changes_to_mechlab(mchanges, changes, inventory);
                }
            }

            var dchanges = DefaultFixer.Instance.GetDefaultsChange(mech, inventory, CategoryID);
            if (dchanges != null)
                changes_to_mechlab(dchanges, changes, inventory);

            return false;
        }
    }
}