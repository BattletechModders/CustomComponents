using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BattleTech;
using HBS.Util;

namespace CustomComponents
{
    public class CustomComponentFactory<TCustomComponent> : ICustomComponentFactory
        where TCustomComponent : class, ICustomComponent, new()
    {
        public string ComponentSectionName { get; set; }

        public virtual ICustomComponent Create(MechComponentDef target, Dictionary<string, object> values)
        {
            if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
            {
                return null;
            }
            
            if (!(customSettingsObject is Dictionary<string, object> customSettings))
            {
                return null;
            }

            if (!customSettings.TryGetValue(ComponentSectionName, out var componentSettingsObject))
            {
                return null;
            }
            
            if (!(componentSettingsObject is Dictionary<string, object> componentSettings))
            {
                return null;
            }

            var obj = new TCustomComponent();
            JSONSerializationUtility.RehydrateObjectFromDictionary(obj, componentSettings);
            return obj;
        }
    }
}