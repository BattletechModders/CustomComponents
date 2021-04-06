using BattleTech.UI;
using fastJSON;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("Flags")]
    public class Flags : SimpleCustomComponent
    {
        public List<string> flags;
        public string ErrorCannotRemove { get; set; }
        public string ErrorItemBroken { get; set; }
        public string ErrorItemDestroyed { get; set; }
        public string ErrorInvalid { get; set; }


        public override string ToString()
        {
            return flags.Aggregate("Flags: [", (current, flag) => current + flag + " ") + "]";
        }
    }
}
