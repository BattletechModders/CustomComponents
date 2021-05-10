using System.Collections.Generic;

namespace CustomComponents
{
    public interface IValueComponent
    {
        void LoadValue(object value);
    }

    public class ValueCustomFactory<TCustom, TDef> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, IValueComponent, new()
        where TDef : class
    {
        public string CustomName { get; }
        public ValueCustomFactory(string customName)
        {
            CustomName = customName;
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
                    obj.LoadValue(componentSettingsObject);

                    yield return obj;
                }
            }
            else
            {
                var obj = new TCustom();
                obj.Def = def;
                obj.LoadValue(componentSettingsObject);

                yield return obj;
            }
        }
    }
}