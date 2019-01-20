using System.Collections.Generic;
using HBS.Util;

namespace CustomComponents
{
    public class ArrayCustomFactory<TCustom, TDef> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, new()
        where TDef : class
    {
        public string CustomName { get; }


        public ArrayCustomFactory(string customName)
        {
            CustomName = customName;
        }

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

            if (!(componentSettingsObject is IEnumerable<object> componentSettings))
            {
                yield break;
            }

            foreach (var item in componentSettings)
            {
                if (item is Dictionary<string, object> component)
                {
                    var obj = new TCustom();

                    JSONSerializationUtility.RehydrateObjectFromDictionary(obj, component);
                    obj.Def = def;

                    yield return obj;
                }
            }
        }


        public override string ToString()
        {
            return CustomName + ".ArrayFactory";
        }
    }
}