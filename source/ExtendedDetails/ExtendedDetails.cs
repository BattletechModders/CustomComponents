using BattleTech;
using fastJSON;
using Harmony;
using System;
using System.Collections.Generic;
using ErosionBrushPlugin;

namespace CustomComponents.ExtendedDetails
{

    // this custom is not be used directly from jsons, this is a helper component used by any mod that wants to add custom description details
    public class ExtendedDetails : ICustom
    {
        [JsonIgnore]
        public string OriginalDetails { get; private set; }
        [JsonIgnore]
        private readonly SortedSet<ExtendedDetail> Details = new SortedSet<ExtendedDetail>();

        public  DescriptionDef Def;

        public static ExtendedDetails GetOrCreate(MechComponentDef def)
        {
            return def.GetOrCreate(() => new ExtendedDetails(def.Description));
        }

        public static ExtendedDetails GetOrCreate(ChassisDef def)
        {
            return def.GetOrCreate(() => new ExtendedDetails(def.Description));
        }

        //[Obsolete] // remove
        //public ExtendedDetails(MechComponentDef def) : this(def.Description)
        [Obsolete] // remove
        public ExtendedDetails(MechComponentDef def) : this(def.Description)
        {
        }

        //[Obsolete] // make private
        private ExtendedDetails(DescriptionDef def)
        {
            Def = def;
            OriginalDetails = Def.Details;
            var original = new ExtendedDetail
            {
                UnitType = UnitType.UNDEFINED,
                Index = 0,
                Text = OriginalDetails
            };
            AddDetail(original);
        }

        public void AddDetail(ExtendedDetail detail)
        {
            //Control.Log(Def.UIName + " added " + detail.Text);
            Details.Add(detail);
            SetDescriptionDetails();
            //Control.Log("current :\n" + Def.Details);
        }

        public void RefreshDetails()
        {
            SetDescriptionDetails();
        }

        private void SetDescriptionDetails()
        {
            var details = Details.Join(x => x.Text, "");
            Traverse.Create(Def).Property<string>(nameof(DescriptionDef.Details)).Value = details;
        }

        // this method should be used when wanting custom behavior of showing details
        // if making filters in your mod, make sure those filters are configurable
        // so custom providers can be shown or not shown based on those user configurable filters
        public IEnumerable<ExtendedDetail> GetDetails()
        {
            return Details;
        }
    }

    public class ExtendedDetail : IComparable<ExtendedDetail>
    {
        // UNDEFINED -> always show, in all other cases if actor is known, only show for said actor
        public UnitType UnitType { get; set; }
        
        // some key to identify where the detail come from... traits or critical effects
        // allows to filter in cases where you don't want to show something or only want to show something
        public string Identifier { get; set; }

        public int Index { get; set; } // 0 => original description details, -1 before, 1 after
        public virtual string Text { get; set; }
        
        public int CompareTo(ExtendedDetail other)
        {
            var compared = Index.CompareTo(other.Index);
            if (compared != 0)
            {
                return compared;
            }
            return Identifier.CompareTo(other.Identifier);
        }
    }

    public class ExtendedDetailList : ExtendedDetail
    {
        public string OpenBracket { get; set; }
        public string CloseBracket { get; set; }
        public string Delimiter { get; set; } = ", ";
        public List<string> Values { get; set; } = new List<string>();

        public void Add(string s)
        {
            Values.Add(s);
        }

        public void AddUnique(string s)
        {
            if(Values.Contains(s))
                return;
            Values.Add(s);
        }

        public override string Text
        {
            get => OpenBracket + Values.Join(null, Delimiter) + CloseBracket;

            set
            {

            }
        }
    }
}
