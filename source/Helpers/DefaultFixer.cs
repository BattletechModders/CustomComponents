#undef CCDEBUG
using System.Linq;
using BattleTech;
using HBS.Extensions;

namespace CustomComponents
{
    public static class DefaultFixer
    {
        public static string[] changed_deafult = new string[100];
        public static int num_changed = 0;

        public static string GetID(IDefault item)
        {
            return item.AnyLocation ? item.CategoryID : item.CategoryID + "_" + item.Location;
        }

        static void set_changed(string category)
        {
            changed_deafult[num_changed] = category;
            num_changed += 1;
        }

        static void process_default(MechDef mechDef, IDefault def, SimGameState state)
        {
            string id = GetID(def);
            for (int i = 0; i < num_changed; i++)
                if (changed_deafult[i] == id)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"---- {def.CategoryID}: already changed");
#endif
                    return;
                }

            var component = def.AnyLocation
                ? mechDef.Inventory.FirstOrDefault(i => i.IsCategory(def.CategoryID))
                : mechDef.Inventory.FirstOrDefault(i =>
                    i.IsCategory(def.CategoryID) && def.Location == i.MountedLocation);



            if (component != null)
            {
                if (!component.IsDefault())
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"---- {def.CategoryID}: Found not default {component.ComponentDefID}, skiped");
#endif
                }
                else if (component.IsModuleFixed(mechDef))
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"---- {def.CategoryID}: Found fixed {component.ComponentDefID}, skiped");
#endif
                }
                else if (def.NeedReplaceExistDefault(mechDef, component))
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"---- {def.CategoryID}: Found wrong default {component.ComponentDefID}, replacing with {def}");
#endif
                    var inventory = mechDef.Inventory.ToList();
                    inventory.Remove(component);
                    inventory.Add(def.GetReplace(mechDef, state));
                    mechDef.SetInventory(inventory.ToArray());
                }
                else
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"---- {def.CategoryID}: Found exist default {component.ComponentDefID}, marks as installed");
#endif                   
                }
                set_changed(id);
            }
            else
            {
#if CCDEBUG
                Control.Logger.LogDebug($"---- {def.CategoryID}: not found, adding {def} to {def.Location}");
#endif
                if (def.AddItems(mechDef, state))
                    set_changed(id);
            }
        }

        public static void FixMech(MechDef mechDef, SimGameState state)
        {
            if (mechDef == null)
                return;

            if(Control.Settings.FixDeletedComponents )
                RemoveEmptyRefs(mechDef);


#if CCDEBUG
            Control.Logger.LogDebug($"Default Fixer for {mechDef.Name}({mechDef.Description.Id})");
#endif
            num_changed = 0;

#if CCDEBUG

            Control.Logger.LogDebug($"-- Chassis");
#endif
            if (mechDef.Chassis != null)
                foreach (var def in mechDef.Chassis.GetComponents<ChassisDefaults>())
                {
                    process_default(mechDef, def, state);
                }

#if CCDEBUG
            Control.Logger.LogDebug($"-- Tagged");
#endif

                foreach (var def in DefaultsHandler.Shared.TaggedDefaults)
                {
                    if (mechDef.MechTags.Contains(def.Tag) || mechDef.Chassis.ChassisTags.Contains(def.Tag))
                        process_default(mechDef, def, state);
                }
#if CCDEBUG
            Control.Logger.LogDebug($"-- Other");
#endif

                foreach (var def in DefaultsHandler.Shared.Defaults)
                {
                    process_default(mechDef, def, state);
                }


#if CCDEBUG
            Control.Logger.LogDebug($"-- Changes");

            for (int i =0;i<num_changed;i++)
                Control.Logger.LogDebug($"---- {changed_deafult[i]}");
#endif
        }

        private static void RemoveEmptyRefs(MechDef mechDef)
        {

            if (mechDef.Inventory.Any(i => i?.Def == null))
            {
                Control.Logger.LogError($"Found NULL in {mechDef.Name}({mechDef.Description.Id})");

                foreach (var r in mechDef.Inventory)
                {
                    if (r.Def == null)
                        Control.Logger.LogError($"--- NULL --- {r.ComponentDefID}");
                }
                
                mechDef.SetInventory(mechDef.Inventory.Where(i => i.Def != null).ToArray());
            }
        }

        public static void FixSavedMech(MechDef mechDef, SimGameState state)
        {

            if (Control.Settings.FixDeletedComponents)
                RemoveEmptyRefs(mechDef);
            ReAddFixed(mechDef, state);
            CategoryController.RemoveExcessDefaults(mechDef);

            FixMech(mechDef, state);

        }

        private static void ReAddFixed(MechDef mechDef, SimGameState state)
        {
            mechDef.SetInventory(mechDef.Inventory.Where(i => !i.IsModuleFixed(mechDef)).ToArray());
            mechDef.Refresh();
        }

        public static MechComponentRef GetReplaceFor(MechDef mech, string categoryId, ChassisLocations location, SimGameState state)
        {
            bool check_def(IDefault def)
            {
                return def.CategoryID == categoryId && (def.AnyLocation || location == def.Location);
            }


            foreach (var def in mech.Chassis.GetComponents<ChassisDefaults>())
                if (check_def(def))
                    return def.GetReplace(mech, state);

            if (DefaultsHandler.Shared.TaggedDefaults != null)
                foreach (var def in DefaultsHandler.Shared.TaggedDefaults.Where(check_def))
                    if (mech.MechTags.Contains(def.Tag) || mech.Chassis.ChassisTags.Contains(def.Tag))
                        return def.GetReplace(mech, state);

            return DefaultsHandler.Shared.Defaults != null ? DefaultsHandler.Shared.Defaults.Where(check_def).Select(def => def.GetReplace(mech, state)).FirstOrDefault() : null;
        }

        public static object GetDefId(MechDef mech, string categoryId, ChassisLocations location)
        {
            bool check_def(IDefault def)
            {
                return def.CategoryID == categoryId && (def.AnyLocation || location == def.Location);
            }


            foreach (var def in mech.Chassis.GetComponents<ChassisDefaults>())
                if (check_def(def))
                    return def.DefID;

                foreach (var def in DefaultsHandler.Shared.TaggedDefaults.Where(check_def))
                    if (mech.MechTags.Contains(def.Tag) || mech.Chassis.ChassisTags.Contains(def.Tag))
                        return def.DefID;

                return DefaultsHandler.Shared.Defaults.Where(check_def).Select(def => def.DefID).FirstOrDefault();
        }
    }
}