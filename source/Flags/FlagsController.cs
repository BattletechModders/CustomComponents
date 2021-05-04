using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomComponents
{
    public delegate bool CheckFlagDelegate(MechComponentDef item);

    public class FlagsController<T>
        where T : class, new()
    {
        private class FlagInfo
        {
            public string Name { get; set; }
            public string[] ChildFlags { get; set; }

            public List<FlagInfo> Childs { get; set; }
            public SetterDelegate Setter { get; set; }

            public CustomSetterDelegate CustomSetter { get; set; }
        }

        private static Dictionary<string, FlagInfo> flags;

        static FlagsController()
        {
            flags = new Dictionary<string, FlagInfo>();

            var type = typeof(T);

            foreach (var propertyInfo in type.GetProperties())
            {
                var flag_attribute = propertyInfo.GetCustomAttribute<CustomFlagAttribute>();
                if (flag_attribute == null)
                    continue;

                var child_attribute = propertyInfo.GetCustomAttribute<SubFlagsAttribute>();

                var flag = new FlagInfo()
                {
                    Name = flag_attribute.FlagName,
                    Setter = (obj, value) => propertyInfo.SetValue(obj, value),
                    ChildFlags = child_attribute?.Childs
                };

                flags[flag.Name] = flag;
            }

            if (flags.Count == 0)
                Control.LogError($"{type} cannot be used as CustomFlags, no flags");

            foreach (var flagInfo in flags)
            {
                var child = flagInfo.Value.ChildFlags;
                if (child == null || child.Length == 0)
                    flagInfo.Value.Childs = null;
                else
                    flagInfo.Value.Childs = child
                        .Select(i => flags.TryGetValue(i, out var f) ? f : null)
                        .Where(i => i != null)
                        .ToList();
            }

            foreach (var minfo in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic))
            {
                var setter_attribute = minfo.GetCustomAttribute<CustomSetterAttribute>();
                if (setter_attribute == null)
                    continue;

                if(!flags.TryGetValue(setter_attribute.Flag, out var f))
                {
                    Control.LogError(
                        $"{minfo.Name} marked as setter for {setter_attribute.Flag} but there is not such tag");
                    continue;
                }

                if (minfo.ReturnType != typeof(bool))
                {
                    Control.LogError(
                        $"{minfo.Name} marked as setter for {setter_attribute.Flag} but not return Boolean");
                    continue;
                }

                var prams = minfo.GetParameters();
                if(prams.Length != 1)
                {
                    Control.LogError(
                        $"{minfo.Name} marked as setter for {setter_attribute.Flag} but have wrong parameters");
                    continue;
                }

                if (prams[0].ParameterType != typeof(MechComponentDef))
                {
                    Control.LogError(
                        $"{minfo.Name} marked as setter for {setter_attribute.Flag} but have wrong parameters");
                    continue;
                }

                f.CustomSetter = (obj, item) => (bool)minfo.Invoke(obj, new[] {item});
            }
        }

        private delegate void SetterDelegate(T obj, bool value);
        private delegate bool CustomSetterDelegate(T obj, MechComponentDef item);

        private static Dictionary<string, T> flags_database = new Dictionary<string, T>();


        private static FlagsController<T> _shared;
        public static FlagsController<T> Shared
        {
            get
            {
                if(_shared == null)
                    _shared = new FlagsController<T>();
                return _shared;
            }
        }

        public T this[MechComponentDef item]
        {
            get
            {
                if (item == null)
                    return null;

                if (!flags_database.TryGetValue(item.Description.Id, out var flags))
                {
                    flags = BuildFlags(item);
                    flags_database[item.Description.Id] = flags;
                }

                return flags;
            }
        }

        private T BuildFlags(MechComponentDef item)
        {
            void set_recursive(FlagInfo flag, Dictionary<string, bool> values)
            {
                values[flag.Name] = true;
                if(flag.Childs != null)
                    foreach (var flagChild in flag.Childs)
                        set_recursive(flagChild, values);
            }

            Control.LogDebug(DType.Flags, "BuildFlags for " + item.Description.Id);
            var result = new T();
            var f = item.GetComponent<Flags>();

            var temp_f = flags.ToDictionary(i => i.Key, i => false);

            if (f != null)
                foreach (var flag in flags)
                    temp_f[flag.Key] = f.flags.Contains(flag.Key);

            foreach (var flag in flags)
            {
                var value = temp_f[flag.Key];
                if (!value && flag.Value.CustomSetter != null)
                    value = flag.Value.CustomSetter(result, item);

                if(value)
                    set_recursive(flag.Value, temp_f);
            }

            foreach (var flag in flags)
            {
                flag.Value.Setter(result, temp_f[flag.Key]);
            }

            if(Control.Settings.DebugInfo.HasFlag(DType.Flags))
                Control.LogDebug(DType.Flags, $"Flags for {item.Description.Id}: [{result.ToString()}]");

            return result;
        }

    }
}
