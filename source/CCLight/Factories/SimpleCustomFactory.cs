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

        public virtual ICustom Create(object target, Dictionary<string, object> values)
        {
            if (!(target is TDef def))
            {
                return null;
            }

            if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
            {
                return null;
            }
            
            if (!(customSettingsObject is Dictionary<string, object> customSettings))
            {
                return null;
            }

            if (!customSettings.TryGetValue(CustomName, out var componentSettingsObject))
            {
                return null;
            }
            
            if (!(componentSettingsObject is Dictionary<string, object> componentSettings))
            {
                return null;
            }

            var obj = new TCustom();
            JSONSerializationUtility.RehydrateObjectFromDictionary(obj, componentSettings);
            obj.Def = def;

            return obj;
        }
    }
}