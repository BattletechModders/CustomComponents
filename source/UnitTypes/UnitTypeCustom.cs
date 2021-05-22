using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    [CustomComponent("UnitType")]
    public class UnitTypeCustom : SimpleCustomChassis, IListComponent<string>
    {
        public HashSet<string> Types;

        public void LoadList(IEnumerable<string> items)
        {
            Types = items.ToHashSet();
        }
    }

    [CustomComponent("UnitTypeAdd")]
    public class UnitTypeAddCustom : SimpleCustomChassis, IListComponent<string>
    {
        public HashSet<string> Types;

        public void LoadList(IEnumerable<string> items)
        {
            Types = items.ToHashSet();
        }
    }
}