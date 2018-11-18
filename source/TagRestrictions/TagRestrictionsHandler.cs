using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
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
            return ValidateMech(mechDef);
        }

        internal void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            ValidateMech(mechDef, errors);
        }

        internal bool ValidateMech(MechDef mechDef, Dictionary<MechValidationType, List<Text>> errors = null)
        {
            var valid = true;
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

            var tags = new HashSet<string>();

            // chassis
            {
                var chassis = mechDef.Chassis;
                // tags
                if (chassis.ChassisTags != null)
                {
                    tags.UnionWith(chassis.ChassisTags);
                }

                // id
                var identifier = chassis.Description.Id;
                tags.Add(identifier);
                AddNameForTag(identifier, chassis.Description.UIName);
            }

            // components
            foreach (var def in mechDef.Inventory.Select(r => r.Def))
            {
                // tags
                if (def.ComponentTags != null)
                {
                    tags.UnionWith(def.ComponentTags);
                }

                // id
                var identifier = def.Description.Id;
                tags.Add(identifier);
                AddNameForTag(identifier, def.Description.UIName);

                // category for component
                var category = def.GetComponent<Category>();
                if (category != null)
                {
                    var categoryDescriptor = Control.GetCategory(category.CategoryID);

                    // category id
                    tags.Add(category.CategoryID);
                    if (categoryDescriptor != null)
                    {
                        AddNameForTag(category.CategoryID, categoryDescriptor.DisplayName);
                    }
                    
                    // category tag
                    if (category.Tag != null)
                    {
                        tags.Add(category.Tag);
                        if (categoryDescriptor != null)
                        {
                            AddNameForTag(category.Tag, categoryDescriptor.DisplayName);
                        }
                    }
                }
            }

            foreach (var tag in tags)
            {
                foreach (var requiredTag in RequiredTags(tag))
                {
                    if (tags.Contains(requiredTag))
                    {
                        continue;
                    }

                    if (errors == null)
                    {
                        return false;
                    }

                    valid = false;
                    var tagName = NameForTag(tag);
                    var requiredTagName = NameForTag(requiredTag);
                    errors[MechValidationType.InvalidInventorySlots].Add(new Text($"{tagName} requires {requiredTagName}"));
                }

                foreach (var incompatibleTag in IncompatibleTags(tag))
                {
                    if (!tags.Contains(incompatibleTag))
                    {
                        continue;
                    }

                    if (errors == null)
                    {
                        return false;
                    }

                    valid = false;
                    var tagName = NameForTag(tag);
                    var incompatibleTagName = NameForTag(incompatibleTag);
                    errors[MechValidationType.InvalidInventorySlots].Add(new Text($"{tagName} can't be used with {incompatibleTagName}"));
                }
            }

            return valid;
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