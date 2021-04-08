using System.Collections.Generic;

namespace CustomComponents
{
    public class CategoryDefaultsAdjust : IAdjustChange
    {
        public string CategoryID { get; private set; }
        public string ChangeID => "Category_" + CategoryID;

        public CategoryDefaultsAdjust(string categoryId)
        {
            CategoryID = categoryId;
        }
        public void DoAdjust(MechLabHelper mechLab, LocationHelper loc, List<IChange> changes)
        {
        }
    }
}