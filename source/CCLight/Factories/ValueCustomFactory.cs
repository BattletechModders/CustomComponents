using System;
using System.Collections.Generic;

namespace CustomComponents
{
    public interface IValueComponent<T>
    {
        void LoadValue(T value);
    }

    public class ValueCustomFactory<TCustom, TDef, TValue> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, IValueComponent<TValue>, new()
        where TDef : class
    {
        public string CustomName { get; }
        public ValueCustomFactory(string customName)
        {
            CustomName = customName;
        }

        private TValue parse(object val, object def)
        {
            if (val == null)
            {
                Control.LogError($"{CustomName} for {Database.Identifier(def)} has null, used {default(TValue)}");
                return default;
            }

            try
            {
                return (TValue)Convert.ChangeType(val, typeof(TValue));
            }
            catch (Exception e)
            {
                Control.LogError($"Can't convert value to type '{typeof(TValue).FullName}' for custom '{CustomName}' in def '{Database.Identifier(def)}'", e);
                return default;
            }
        }

        public IEnumerable<ICustom> Create(object target, Dictionary<string, object> values)
        {
            if (!(target is TDef def))
            {
                yield break;
            }

            if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
            {
                yield break;
            }

            if (!(customSettingsObject is Dictionary<string, object> customSettings))
            {
                yield break;
            }

            if (!customSettings.TryGetValue(CustomName, out var componentSettingsObject))
            {
                yield break;
            }

            if ((componentSettingsObject is IEnumerable<object> compList))
            {
                foreach (var item in compList)
                {
                    var obj = new TCustom();
                    obj.Def = def;
                    obj.LoadValue(parse(item, def));

                    yield return obj;
                }
            }
            else
            {
                var obj = new TCustom();
                obj.Def = def;
                obj.LoadValue(parse(componentSettingsObject, def));

                yield return obj;
            }
        }
    }

    public class EnumValueCustomFactory<TCustom, TDef, TValue> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, IValueComponent<TValue>, new()
        where TDef : class
        where TValue : struct
    {
        public string CustomName { get; }
        public EnumValueCustomFactory(string customName)
        {
            CustomName = customName;
        }

        private TValue parse(object val, object def)
        {
            if (val == null)
            {
                Control.LogError($"{CustomName} for {Database.Identifier(def)} has null, used {default(TValue)}");
                return default;
            }

            if (val is string str)
                if (Enum.TryParse(str, true, out TValue res))
                    return res;
                else
                {
                    Control.LogError($"{val} is wrong value for {CustomName} for {Database.Identifier(def)} used {default(TValue)}");
                    return default;
                }

            try
            {
                return (TValue)val;
            }
            catch (Exception e)
            {
                Control.LogError(
                    $"{val} is wrong value for {CustomName} for {Database.Identifier(def)} used {default(TValue)}",
                    e);
                return default;
            }
        }

        public IEnumerable<ICustom> Create(object target, Dictionary<string, object> values)
        {
            if (!(target is TDef def))
            {
                yield break;
            }

            if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
            {
                yield break;
            }

            if (!(customSettingsObject is Dictionary<string, object> customSettings))
            {
                yield break;
            }

            if (!customSettings.TryGetValue(CustomName, out var componentSettingsObject))
            {
                yield break;
            }

            if ((componentSettingsObject is IEnumerable<object> compList))
            {
                foreach (var item in compList)
                {
                    var obj = new TCustom();
                    obj.Def = def;
                    obj.LoadValue(parse(item, def));

                    yield return obj;
                }
            }
            else
            {
                var obj = new TCustom();
                obj.Def = def;
                obj.LoadValue(parse(componentSettingsObject, def));

                yield return obj;
            }
        }

      
    }

}

