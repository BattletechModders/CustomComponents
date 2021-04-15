using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    public class CategoryDefaultsAdjust : IDelayChange
    {
        public string CategoryID { get; private set; }
        public string ChangeID => "Category_" + CategoryID;

        public CategoryDefaultsAdjust(string categoryId)
        {
            CategoryID = categoryId;
        }

        public bool DoAdjust(Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            var mech = MechLabHelper.CurrentMechLab.ActiveMech;

            var record = DefaultsDatabase.Instance[mech];
            if (record == null || mech == null)
                return false;

            if (record.Multi != null && record.Multi.UsedCategories.ContainsKey(CategoryID))
            {
                var mchanges = DefaultFixer.Instance.GetMultiChange(mech, inventory);
                if (mchanges != null )
                {
                    changes_to_mechlab(mchanges, changes, inventory);
                }
            }

            return false;
        }

        private void changes_to_mechlab(List<DefaultFixer.inv_change> inv_changes, Queue<IChange> changes, List<SlotInvItem> inventory)
        {

        }
    }
}