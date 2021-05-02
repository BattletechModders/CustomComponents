using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CustomComponents
{
    public interface IListComponent
    {
        void LoadList(IEnumerable<object> items);
    }

    public class ListCustomFactory<TCustom, TDef> : ICustomFactory
        where TCustom : SimpleCustom<TDef>, IListComponent, new()
        where TDef : class
    {
        public string CustomName { get; }
        public ListCustomFactory(string customName)
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

            if (!(componentSettingsObject is IEnumerable<object> compList))
            {
                yield break;
            }

            var obj = new TCustom();
            obj.LoadList(compList);

            yield return obj;
        }


    }
}