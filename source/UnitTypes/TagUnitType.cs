using BattleTech;
using HBS.Collections;

namespace CustomComponents
{
    public class TagUnitType : IUnitType
    {
        public string Name { get; set; }

        public string[] RequiredTags { get; set; }
        public string[] ForbiddenTags { get; set; }

        public bool IsThisType(MechDef mechdef)
        {
            if (mechdef?.Chassis == null)
                return false;
            
            TagSet tags = new TagSet();
            if(mechdef.MechTags != null)
                tags.UnionWith(mechdef.MechTags);

            if(mechdef.Chassis.ChassisTags != null)
                tags.UnionWith(mechdef.Chassis.ChassisTags);

            if(RequiredTags != null && RequiredTags.Length > 0)
                foreach (var tag in RequiredTags)
                    if (!tags.Contains(tag))
                        return false;

            if (ForbiddenTags != null && ForbiddenTags.Length > 0)
                foreach (var tag in ForbiddenTags)
                    if (tags.Contains(tag))
                        return false;

            return true;
        }
    }
}