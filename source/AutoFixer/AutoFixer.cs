using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    public delegate void AutoFixerDelegate(MechDef mechDef, SimGameState simgame);


    public class AutoFixer
    {
        public static AutoFixer Shared = new AutoFixer();

        private List<AutoFixerDelegate> fixers = new List<AutoFixerDelegate>();
        private List<AutoFixerDelegate> savegamefixers = new List<AutoFixerDelegate>();


        internal void FixMechDef(MechDef mechDef, SimGameState state)
        {
            if(mechDef != null)
                foreach (var autoFixerDelegate in fixers)
                {
                    try
                    {
                        autoFixerDelegate(mechDef, null);
                    }
                    catch (Exception e)
                    {
                        Control.Logger.LogError($"Exception in Autofixer {autoFixerDelegate.Method.Name}", e);
                    }
                }
        }

        internal void FixSavedMech(MechDef mechDef, SimGameState state)
        {
            if (mechDef != null)
                foreach (var autoFixerDelegate in savegamefixers)
                {
                    try
                    {
                        autoFixerDelegate(mechDef, state);
                    }
                    catch (Exception e)
                    {
                        Control.Logger.LogError($"Exception in Autofixer {autoFixerDelegate.Method.Name}", e);
                    }
                }
        }

        public void RegisterMechFixer(AutoFixerDelegate fixer)
        {
            if (fixer != null)
            {
                fixers.Add(fixer);
                savegamefixers.Add(fixer);
            }
        }

        public void RegisterSaveMechFixer(AutoFixerDelegate fixer)
        {
            if (fixer != null)
            {
                savegamefixers.Add(fixer);
            }
        }

        internal void RemoveEmptyRefs(MechDef mechDef, SimGameState state)
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


        internal void ReAddFixed(MechDef mechDef, SimGameState state)
        {
            mechDef.SetInventory(mechDef.Inventory.Where(i => !i.IsModuleFixed(mechDef)).ToArray());
            mechDef.Refresh();
        }

    }
}