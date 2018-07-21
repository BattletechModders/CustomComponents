using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public class SimpleCustomComponentFactory<TCustomComponent> : CustomComponentFactory<TCustomComponent>
        where TCustomComponent : SimpleCustomComponent, new()
    {
        public override ICustomComponent Create(MechComponentDef target, Dictionary<string, object> values)
        {
            var obj = base.Create(target, values) as TCustomComponent;
            if (obj != null)
            {
                obj.Def = target;
                if (obj is IAfterLoad load)
                {
                    //Control.Logger.LogDebug($"IAfterLoad: {obj.Def.Description.Id}");
                    load.OnLoaded(values);
                }
            }
            return obj;
        }
    }
}