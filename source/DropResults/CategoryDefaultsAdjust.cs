using System.Collections.Generic;

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
        }
    }
}