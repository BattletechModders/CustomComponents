using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomComponents
{
    public delegate bool CheckFlagDelegate(MechComponentDef item);

    public class FlagsController
    {
        private class flag_record
        {
            public string name;

            public CheckFlagDelegate check_flag;
            public flag_record parent_flag;
            public bool has_parent { get => parent_flag == null; }
            public flag_record[] child_flags;
            public bool has_children { get => child_flags == null || child_flags.Length == 0; }
        }

        private static FlagsController _instance;
        private Dictionary<string, Dictionary<string, bool>> flags_database = new Dictionary<string, Dictionary<string, bool>>;
        private HashSet<string> all_flags = new HashSet<string>();
        private List<flag_record> flag_records;


        public static FlagsController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlagsController();
                return _instance;
            }
        }


        public bool this[MechComponentDef item, string flag]
        {
            get
            {
                if (!all_flags.Contains(flag) || item == null)
                    return false;

                if (!flags_database.TryGetValue(item.Description.Id, out var flags))
                {
                    flags = BuildFlags(item);
                    flags_database[item.Description.Id] = flags;
                }

                return flags[flag];
            }
        }

        private Dictionary<string, bool> BuildFlags(MechComponentDef item)
        {
            var result = new Dictionary<string, bool>();
            foreach (var record in flag_records)
                result[record.name] = record.check_flag(item);

            foreach(var record in flag_records.Where(i => i.has_children))
            {
                if(result[record.name])
                {
                    foreach (var subrecord in record.child_flags)
                        result[subrecord.name] = true;
                }
                else
                {
                    var r = true;
                    foreach(var subrecord in record.child_flags)
                    {
                        if(!result[subrecord.name])
                        {
                            r = false;
                            break;
                        }
                    }
                    result[record.name] = r;
                }
            }

            return result;
        }

        public void RegisterFlag(string name, CheckFlagDelegate check_delegate = null, string[] subflags = null)
        {
            var record = new flag_record();
            record.name = name;
            
            if(check_delegate == null)
                record.check_flag = (item) => item.Is<Flags>(out var f) && f.flags.Contains(name);
            else
                record.check_flag = (item) => item.Is<Flags>(out var f) && f.flags.Contains(name) || check_delegate(item);

            if (subflags != null)
            {
                record.child_flags = flag_records.Where(i => subflags.Contains(i.name)).ToArray();
                foreach (var item in record.child_flags)
                    item.parent_flag = record;
            }

            flag_records.Add(record);
            all_flags.Add(record.name);
        }
    }
}
