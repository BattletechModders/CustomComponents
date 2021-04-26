using BattleTech;
using Harmony;
using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    //class AdjustDescriptionPreProcessor : IPreProcessor
    //{
    //    public void PreProcess(object target, Dictionary<string, object> values)
    //    {
    //        if (target is MechComponentDef def)
    //        {
    //            var description = def.GetComponents<IAdjustDescription>()
    //                .Aggregate("", (current, adjuster) => adjuster.AdjustDescription(current));

    //            if (description != "")
    //            {
    //                var trav = new Traverse(def.Description).Property<string>("Details");
    //                trav.Value = def.Description.Details + "\n" + description;
    //            }

    //            //TagRestrictionsHandler.Shared.ProcessDescription(def.ComponentTags, def.Description);
    //        }
    //        //else if(target is MechDef mdef)
    //        //    TagRestrictionsHandler.Shared.ProcessDescription(mdef.MechTags, mdef.Description);
    //        //else if (target is ChassisDef cdef)
    //        //    TagRestrictionsHandler.Shared.ProcessDescription(cdef.ChassisTags, cdef.Description);
    //    }
    //}
}
