using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Localize;

namespace CustomComponents
{
    internal class TagRestrictionsHandler
    {
        internal static TagRestrictionsHandler Shared = new TagRestrictionsHandler();

        private Dictionary<string, TagRestrictions> Restrictions { get; } = new Dictionary<string, TagRestrictions>();

        internal void Add(TagRestrictions restrictions)
        {
            Restrictions.Add(restrictions.Tag, restrictions);
        }

        internal bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            return ValidateMech(out var error, mechDef);
        }

        internal void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            ValidateMech(out var error, mechDef, errors: errors);
        }

        internal string ValidateDrop(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
            ValidateMech(out var error, mech, drop_item.ComponentRef.Def);
            return error;
        }

        internal bool ValidateMech(
            out string error,
            MechDef mechDef,
            MechComponentDef droppedComponent = null,
            Dictionary<MechValidationType, List<Text>> errors = null)
        {
            error = null;
            var tagsUINames = new Dictionary<string, string>();
            void AddNameForTag(string tag, string UIName)
            {
                try
                {
                    tagsUINames.Add(tag, UIName);
                }
                catch (ArgumentException)
                {
                }
            }

            string NameForTag(string tag)
            {
                if (!tagsUINames.TryGetValue(tag, out var UIName))
                {
                    UIName = tag;
                }

                return UIName;
            }

            var tagsOnMech = new HashSet<string>();

            // chassis
            {
                var chassis = mechDef.Chassis;
                // tags
                if (chassis.ChassisTags != null)
                {
                    tagsOnMech.UnionWith(chassis.ChassisTags);
                }

                // id
                var identifier = chassis.Description.Id;
                tagsOnMech.Add(identifier);
                AddNameForTag(identifier, chassis.Description.UIName);
            }

            void ProcessComponent(MechComponentDef def, HashSet<string> tagsForComponent)
            {
                // tags
                if (def.ComponentTags != null)
                {
                    tagsForComponent.UnionWith(def.ComponentTags);
                }

                // id
                var identifier = def.Description.Id;
                tagsForComponent.Add(identifier);
                AddNameForTag(identifier, def.Description.UIName);

                // category for component
                var category = def.GetComponent<Category>();
                if (category != null)
                {
                    var categoryDescriptor = Control.GetCategory(category.CategoryID);

                    // category id
                    tagsForComponent.Add(category.CategoryID);
                    if (categoryDescriptor != null)
                    {
                        AddNameForTag(category.CategoryID, categoryDescriptor.DisplayName);
                    }
                    
                    // category tag
                    if (category.Tag != null)
                    {
                        tagsForComponent.Add(category.Tag);
                        if (categoryDescriptor != null)
                        {
                            AddNameForTag(category.Tag, categoryDescriptor.DisplayName);
                        }
                    }
                }
            }

            // components
            foreach (var def in mechDef.Inventory.Select(r => r.Def))
            {
                ProcessComponent(def, tagsOnMech);
            }

            HashSet<string> tagsForDropped = null;
            if (droppedComponent != null)
            {
                tagsForDropped = new HashSet<string>();
                ProcessComponent(droppedComponent, tagsForDropped);
                tagsOnMech.UnionWith(tagsForDropped); // used for incompatible check
            }

            foreach (var tag in tagsForDropped ?? tagsOnMech)
            {
                var requiredTags = RequiredTags(tag);
                foreach (var requiredTag in requiredTags)
                {
                    if (tagsOnMech.Contains(requiredTag))
                    {
                        continue;
                    }

                    var tagName = NameForTag(tag);
                    var requiredTagName = NameForTag(requiredTag);
                    error = $"{tagName} requires {requiredTagName}";

                    if (errors == null)
                    {
                        return false;
                    }

                    errors[MechValidationType.InvalidInventorySlots].Add(new Text(error));
                }
            }

            foreach (var tag in tagsOnMech)
            {
                var incompatibleTags = IncompatibleTags(tag);
                foreach (var incompatibleTag in incompatibleTags)
                {
                    // if dropped, we either want only to check against dropped or the dropped against everything
                    var checkedTags = tagsForDropped != null && !tagsForDropped.Contains(tag) ? tagsForDropped : tagsOnMech;
                    if (!checkedTags.Contains(incompatibleTag))
                    {
                        continue;
                    }
                    
                    var tagName = NameForTag(tag);
                    var incompatibleTagName = NameForTag(incompatibleTag);
                    error = $"{tagName} can't be used with {incompatibleTagName}";

                    if (errors == null)
                    {
                        return false;
                    }
                    
                    errors[MechValidationType.InvalidInventorySlots].Add(new Text(error));
                }
            }

            return error == null;
        }

        private IEnumerable<string> RequiredTags(string tag)
        {
            if (!Restrictions.TryGetValue(tag, out var restriction))
            {
                yield break;
            }

            if (restriction.RequiredTags == null)
            {
                yield break;
            }

            foreach (var requiredTag in restriction.RequiredTags)
            {
                yield return requiredTag;
            }
        }

        private IEnumerable<string> IncompatibleTags(string tag)
        {
            if (!Restrictions.TryGetValue(tag, out var restriction))
            {
                yield break;
            }

            if (restriction.IncompatibleTags == null)
            {
                yield break;
            }

            foreach (var incompatibleTag in restriction.IncompatibleTags)
            {
                yield return incompatibleTag;
            }
        }
    }
}