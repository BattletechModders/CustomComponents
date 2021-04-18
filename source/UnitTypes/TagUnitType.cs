using System.Linq;
using System.Text;
using BattleTech;
using HBS.Collections;
using Harmony;

namespace CustomComponents
{
    public class TagUnitType : IUnitType
    {
        public string Name { get; set; }

        public string[] RequiredTags { get; set; }
        public string[] AnyTags { get; set; }
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

            if (AnyTags == null || AnyTags.Length == 0)
                return true;

            return AnyTags.Any(i => tags.Contains(i));
        }

        public override string ToString()
        {
            string show_array(string name, string[] tags)
            {
                string result = "\n- " + name + ": [";
                result += tags.Join(null, " ") + "]";
                return result;
            }

            StringBuilder sb = new StringBuilder("TagUnitType: " + Name);
            if (RequiredTags != null && RequiredTags.Length > 0)
            {
                sb.Append(show_array("RequiredTags", RequiredTags));
            }
            if (ForbiddenTags != null && ForbiddenTags.Length > 0)
            {
                sb.Append(show_array("ForbiddenTags", ForbiddenTags));
            }
            if (AnyTags != null && AnyTags.Length > 0)
            {
                sb.Append(show_array("AnyTags", AnyTags));
            }

            return sb.ToString();
        }
    }
}