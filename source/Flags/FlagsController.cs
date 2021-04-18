using BattleTech;
using Localize;
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
        public class Flag
        {
            private Dictionary<string, bool> flags;

            public Flag(Dictionary<string, bool> flags)
            {
                this.flags = flags;
                if (flags.TryGetValue(CCF.Default, out var v))
                    Default = v;
                else
                    Default = false;
            }
        
            public bool this[string flag]
            {
                get
                {
                    if (flags.TryGetValue(flag, out var v))
                        return v;
                    return false;
                }
            }

            public bool Default { get; internal set; }
        }

        private class flag_record
        {
            public string name;

            public CheckFlagDelegate check_flag;
            public flag_record parent_flag;
            public bool has_parent { get => parent_flag == null; }
            public flag_record[] child_flags;
            public bool has_children { get => child_flags != null && child_flags.Length != 0; }
        }

        private static FlagsController _instance;
        private Dictionary<string, Flag> flags_database = new Dictionary<string, Flag>();
        private HashSet<string> all_flags = new HashSet<string>();
        private List<flag_record> flag_records = new List<flag_record>();


        public static FlagsController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlagsController();
                return _instance;
            }
        }

        public Flag this[MechComponentDef item]
        {
            get
            {
                if (!flags_database.TryGetValue(item.Description.Id, out var flags))
                {
                    flags = BuildFlags(item);
                    flags_database[item.Description.Id] = flags;
                }

                return flags;
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

        private Flag BuildFlags(MechComponentDef item)
        {
            Control.LogDebug(DType.Flags, "BuildFlags for " + item.Description.Id);
            var result = new Dictionary<string, bool>();
            foreach (var record in flag_records)
            {
                var v= record.check_flag(item);
                result[record.name] = v;
                Control.LogDebug(DType.Flags, $"-- {record.name} : {v}");

            }

            foreach (var record in flag_records.Where(i => i.has_children))
            {
                Control.LogDebug(DType.Flags, $"- {record.name} : ChildFlags check");
                if (result[record.name])
                {
                    foreach (var subrecord in record.child_flags)
                        result[subrecord.name] = true;
                    Control.LogDebug(DType.Flags, $"-- true : set all childs");
                }
                else
                {
                    var r = true;
                    foreach (var subrecord in record.child_flags)
                    {
                        if (!result[subrecord.name])
                        {
                            r = false;
                            break;
                        }
                    }
                    result[record.name] = r;
                    Control.LogDebug(DType.Flags, $"-- false : set to {r} by childs");
                }
            }

            return new Flag(result);
        }

        internal bool CanBeFielded(MechDef mechDef)
        {
            foreach (var item in mechDef.Inventory)
            {
                var f = item.Flags();

                if (f[CCF.Invalid])
                    return false;

                if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f[CCF.NotBroken] || f[CCF.NotDestroyed]))
                    return false;

                if (item.DamageLevel == ComponentDamageLevel.Penalized && f[CCF.NotBroken])
                    return false;
            }
            return true;
        }

        internal void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var item in mechDef.Inventory)
            {
                var f = item.Flags();

                if (f[CCF.Invalid])
                    errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                        Control.Settings.Message.Flags_InvaildComponent,  item.Def.Description.Name));

                if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f[CCF.NotBroken] || f[CCF.NotDestroyed]))
                {
                    errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                        Control.Settings.Message.Flags_DestroyedComponent, item.Def.Description.Name));
                }

                if (item.DamageLevel == ComponentDamageLevel.Penalized && f[CCF.NotBroken])
                {
                    errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                        Control.Settings.Message.Flags_DamagedComponent, item.Def.Description.Name));
                }
            }
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
            Control.Log("Flag " + name + " registred");
        }
    }
}
