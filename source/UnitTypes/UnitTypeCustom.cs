using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    [CustomComponent("UnitType")]
    public class UnitTypeCustom : SimpleCustomChassis, IListComponent
    {
        public HashSet<string> Types;

        public void LoadList(IEnumerable<object> items)
        {
            Types = items.Select(i => i.ToString()).ToHashSet();
        }
    }

    [CustomComponent("UnitTypeAdd")]
    public class UnitTypeAddCustom : SimpleCustomChassis, IListComponent
    {
        public HashSet<string> Types;

        public void LoadList(IEnumerable<object> items)
        {
            Types = items.Select(i => i.ToString()).ToHashSet();
        }
    }
}