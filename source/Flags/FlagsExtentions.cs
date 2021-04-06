using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomComponents
{
    public static class FlagsExtentions
    {
        public static bool HasFlag(this MechComponentDef item, string flag)
        {
            return item == null ? false : FlagsController.Instance[item, flag];
        }
        public static bool HasFlag(this MechComponentRef item, string flag)
        {
            return item == null ? false : FlagsController.Instance[item.Def, flag];
        }

        public static FlagsController.Flag Flags(this MechComponentDef item)
        {
            return item == null ? null : FlagsController.Instance[item];
        }
        public static FlagsController.Flag Flags(this MechComponentRef item)
        {
            return item == null ? null : FlagsController.Instance[item.Def];
        }
    }
}
