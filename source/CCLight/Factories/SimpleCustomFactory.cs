#undef CCDEBUG
using System.Collections.Generic;
using HBS.Util;

namespace CustomComponents
{


    public class SimpleCustomFactory<TCustom, TDef> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, new()
        where TDef : class
    {
        public SimpleCustomFactory(string customName)
        {
            CustomName = customName;
        }

        public string CustomName { get; }

        public virtual IEnumerable<ICustom> Create(object target, Dictionary<string, object> values)
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

            if (componentSettingsObject == null)
            {
                yield break;
            }


#if CCDEBUG
            Control.Logger.LogDebug($"Factory {CustomName} for {customSettingsObject})");
#endif
            if (componentSettingsObject is Dictionary<string, object> compDictionary)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"-- Dictionary - return one {compDictionary}");
                foreach (var pair in compDictionary)
                {
                    Control.Logger.LogDebug($"---- {pair.Key}: {pair.Value}");
                }
#endif
                var obj = new TCustom();
                JSONSerializationUtility.RehydrateObjectFromDictionary(obj, compDictionary);
                obj.Def = def;

                yield return obj;
            }
            else if (componentSettingsObject is IEnumerable<object> compList)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"-- List - return {compList.Count()} items {compList}");
#endif
                foreach (var item in compList)
                {
                    if (item is Dictionary<string, object> compDictItem)
                    {
                        var obj = new TCustom();
                        JSONSerializationUtility.RehydrateObjectFromDictionary(obj, compDictItem);
                        obj.Def = def;

#if CCDEBUG
                        Control.Logger.LogDebug($"---- Factory for {obj}");
#endif

                        yield return obj;
                    }
                }
            }

        }

        public override string ToString()
        {
            return CustomName + ".SimpleFactory";
        }
    }
}