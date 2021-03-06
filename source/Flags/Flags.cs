﻿using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent, IListComponent<string>
    {
        public HashSet<string> flags;

        public override string ToString()
        {
            return flags.Aggregate("Flags: [", (current, flag) => current + flag + " ") + "]";
        }

        public void LoadList(IEnumerable<string> items)
        {
            flags = items.ToHashSet();
        }
    }
}
