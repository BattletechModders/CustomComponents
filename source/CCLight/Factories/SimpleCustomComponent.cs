using BattleTech;
using fastJSON;
using System;

namespace CustomComponents
{
    public class SimpleCustomComponent : ICustomComponent
    {
        [JsonIgnore]
        private WeakReference _def;
        [JsonIgnore]
        private ComponentType type;
        [JsonIgnore]
        private string id;

       [JsonIgnore]
        public MechComponentDef Def
        {
            get
            {
                if (_def == null)
                    return null;
                if(!_def.IsAlive)
                {
                    var def = Database.RefreshDef(id, type);
                    if (def == null)
                        return null;
                    _def.Target = def;

                }
                return _def.Target as MechComponentDef;
            }
            internal set
            {
                if (value == null)
                {
                    _def = null;
                    id = "";
                    type = ComponentType.NotSet;
                } 
                else
                {
                    _def = new WeakReference(value);
                    id = value.Description.Id;
                    type = value.ComponentType;
                }
            }
        }
    }
}