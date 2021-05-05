using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    public class UnitTypeDatabase
    {
        private static UnitTypeDatabase _instance;

        List<IUnitType> types = new List<IUnitType>();
        Dictionary<string, HashSet<string>> known =new Dictionary<string, HashSet<string>>();


        public static UnitTypeDatabase Instance
        {
            get
            {
                if(_instance ==null)
                    _instance = new UnitTypeDatabase();
                return _instance;
            }
        }

        private UnitTypeDatabase()
        {


        }

        public void RegisterUnitType(IUnitType unitType)
        {
            if (unitType == null)
            {
                Control.LogError("Null unit type, skipping");
            }

            var old = types.FirstOrDefault(i => i.Name == unitType.Name);
            if (old != null)
            {
                Control.LogDebug(DType.UnitType, $"-- removed old one");
                types.Remove(old);
            }

            types.Add(unitType);
            Control.Log("UnitType " + unitType.Name + " Registred");
        }

        public HashSet<string> this[MechDef mech] => GetUnitTypes(mech);

        public HashSet<string> GetUnitTypes(MechDef mech)
        {
            HashSet<string> result;

            if (known.TryGetValue(mech.ChassisID, out result))
                return result;

            result = CheckCustom(mech);

            if (result == null)
            {
                result = new HashSet<string>();

                foreach (var unitType in types)
                {
                    if (unitType.IsThisType(mech))
                        result.Add(unitType.Name);
                }
            }

            var add = CheckCustomAdd(mech);

            known[mech.Name] = result;
            return result;
        }

        //Method to patch with other custom type
        public HashSet<string> CheckCustom(MechDef mech)
        {
            return mech.Is<UnitTypeCustom>(out var ut) ? ut.Types?.ToHashSet() : null;
        }

        public HashSet<string> CheckCustomAdd(MechDef mech)
        {
            return mech.Is<UnitTypeAddCustom>(out var ut) ? ut.Types?.ToHashSet() : null;
        }


        public void ShowRegistredTypes()
        {
            if (!Control.Settings.DebugInfo.HasFlag(DType.UnitType))
                return;
            foreach (var unitType in types)
            {
                Control.LogDebug(DType.UnitType, unitType.ToString());
            }
        }
    }

}