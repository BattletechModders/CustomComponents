using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Localize;

namespace CustomComponents
{
    internal class TagsChecker
    {
        private readonly ChassisDef chassisDef;
        private readonly List<MechComponentRef> inventory;

        private readonly Dictionary<ChassisLocations, HashSet<string>> tagsOnLocations = new();
        private readonly HashSet<string> tagsOnMech = new();
        private string error;
        private Dictionary<MechValidationType, List<Text>> errors;

        internal TagsChecker(MechDef mechDef)
        {
            chassisDef = mechDef.Chassis;
            inventory = mechDef.Inventory.ToList();

            CollectChassisTags();
            CollectComponentTags();
        }

        internal string Validate(Dictionary<MechValidationType, List<Text>> errors = null)
        {
            this.errors = errors;
            error = null;

            if (RequiredAnyCheck(tagsOnMech, tagsOnMech)) return error;
            if (RequiredCheck(tagsOnMech, tagsOnMech)) return error;
            if (RequiredOnSameLocationCheck()) return error;

            if (IncompatiblesCheck(tagsOnMech, tagsOnMech)) return error;
            if (IncompatiblesCheckInLocation()) return error;

            return null;
        }

        internal string ValidateDrop(
            bool requiredChecks,
            bool incompatiblesChecks,
            MechComponentRef mechComponentRef,
            ChassisLocations location
        )
        {
            errors = null;
            error = null;

            var componentTags = ProcessComponent(mechComponentRef, location);

            if (requiredChecks)
            {
                if (RequiredAnyCheck(componentTags, tagsOnMech)) return error;
                if (RequiredCheck(componentTags, tagsOnMech)) return error;
                if (RequiredAnyTagsOnSameLocationCheck(componentTags, location)) return error;
                if (RequiredOnSameLocationCheck(componentTags, location)) return error;
            }

            if (incompatiblesChecks)
            {
                if (IncompatiblesCheck(componentTags, tagsOnMech)) return error;
                if (IncompatiblesCheck(tagsOnMech, componentTags)) return error;
                if (IncompatiblesCheckInLocation(componentTags, null, location)) return error;
                if (IncompatiblesCheckInLocation(null, componentTags, location)) return error;
            }

            return null;
        }

        private bool RequiredCheck(HashSet<string> sourceTags, HashSet<string> targetTags)
        {
            if (RequiredCheck(sourceTags, targetTags, RequiredTags)) return true;
            return false;
        }

        private bool RequiredAnyCheck(HashSet<string> sourceTags, HashSet<string> targetTags)
        {
            foreach (var tag in sourceTags)
            {
                var hasMetAnyRequiredAnyTags = true; // no required any tags = ok
                foreach (var requiredAnyTag in RequiredAnyTags(tag))
                {
                    hasMetAnyRequiredAnyTags = false; // at least one required any tag check = nok
                    if (targetTags.Contains(requiredAnyTag))
                    {
                        hasMetAnyRequiredAnyTags = true; // at least on required any tag found = ok
                        break;
                    }
                }

                if (hasMetAnyRequiredAnyTags) continue;

                var tagName = NameForTag(tag);
                if (AddError($"{tagName} requirements are not met")) return true;
            }

            return false;
        }

        private bool RequiredAnyTagsOnSameLocationCheck(HashSet<string> sourceTags, ChassisLocations location)
        {

          if (location == ChassisLocations.None || location == ChassisLocations.All)
            return false;

          if (!tagsOnLocations.TryGetValue(location, out var targetTags))
            targetTags = new HashSet<string>();

          foreach (var tag in sourceTags)
          {
            var hasMetAnyRequiredTags = true;
            foreach(var requiredAnyTag in RequiredAnyTagsOnLocation(tag))
            {
              hasMetAnyRequiredTags = false;
              if (targetTags.Contains(requiredAnyTag))
              {
                hasMetAnyRequiredTags = true;
                break;
              }
            }

            if (hasMetAnyRequiredTags)
              continue;

            var tagName = NameForTag(tag);
            if (AddError($"{tagName} requirements are not met"))
              return true;
          }

          return false;
        }

        private bool RequiredOnSameLocationCheck(HashSet<string> sourceTags = null,
            ChassisLocations location = ChassisLocations.None)
        {
            if (sourceTags != null && location != ChassisLocations.None)
            {
                if (!tagsOnLocations.TryGetValue(location, out var tags)) tags = new HashSet<string>();
                if (RequiredCheck(sourceTags, tags, RequiredTagsOnSameLocation, location)) return true;
            }
            else
            {
                foreach (var tagsOnLocationKeyValue in tagsOnLocations)
                {
                    location = tagsOnLocationKeyValue.Key;
                    var tags = tagsOnLocationKeyValue.Value;

                    if (RequiredCheck(tags, tags, RequiredTagsOnSameLocation, location)) return true;
                }
            }

            return false;
        }

        private bool RequiredCheck(HashSet<string> sourceTags, HashSet<string> targetTags,
            Func<string, IEnumerable<string>> requiredTags, ChassisLocations location = ChassisLocations.None)
        {
            foreach (var tag in sourceTags)
            foreach (var requiredTag in requiredTags(tag))
            {
                if (targetTags.Contains(requiredTag)) continue;

                var tagName = NameForTag(tag);
                var requiredTagName = NameForTag(requiredTag);
                var message = location == ChassisLocations.None
                    ? $"{tagName} requires {requiredTagName}"
                    : $"{tagName} requires {requiredTagName} at {Mech.GetLongChassisLocation(location)}";
                if (AddError(message)) return true;
            }

            return false;
        }

        private bool IncompatiblesCheck(
            HashSet<string> sourceTags,
            HashSet<string> targetTags)
        {
            return IncompatiblesCheck(sourceTags, targetTags, IncompatibleTags);
        }

        private bool IncompatiblesCheckInLocation(
            HashSet<string> sourceTags = null,
            HashSet<string> targetTags = null,
            ChassisLocations location = ChassisLocations.None)
        {
            var locations = location == ChassisLocations.None ? tagsOnLocations.Keys : Enumerable.Repeat(location, 1);

            foreach (var loc in locations)
            {
                if (!tagsOnLocations.TryGetValue(location, out var tags)) tags = new HashSet<string>();
                if (IncompatiblesCheck(sourceTags ?? tags, targetTags ?? tags, IncompatibleTagsOnSameLocation, loc)) return true;
            }

            return false;
        }

        private bool IncompatiblesCheck(
            HashSet<string> sourceTags,
            HashSet<string> targetTags,
            Func<string, IEnumerable<string>> incompatibleTags,
            ChassisLocations location = ChassisLocations.None)
        {
            foreach (var tag in sourceTags)
            foreach (var incompatibleTag in incompatibleTags(tag))
            {
                if (!targetTags.Contains(incompatibleTag)) continue;

                var tagName = NameForTag(tag);
                var incompatibleTagName = NameForTag(incompatibleTag);
                    
                var message = location == ChassisLocations.None
                    ? $"{tagName} can't be used with {incompatibleTagName}"
                    : $"{tagName} can't be used with {incompatibleTagName} at {Mech.GetLongChassisLocation(location)}";
                if (AddError(message)) return true;
            }

            return false;
        }

        private void CollectChassisTags()
        {
            // tags
            if (chassisDef.ChassisTags != null) tagsOnMech.UnionWith(chassisDef.ChassisTags);

            // id
            var identifier = chassisDef.Description.Id;
            tagsOnMech.Add(identifier);
        }

        private void CollectComponentTags()
        {
            foreach (var def in inventory)
            {
                var tagsOnComponent = ProcessComponent(def, def.MountedLocation);
                tagsOnMech.UnionWith(tagsOnComponent);
                AddTagsToLocation(def.MountedLocation, tagsOnComponent);
            }
        }

        private HashSet<string> ProcessComponent(MechComponentRef item, ChassisLocations location)
        {
            HashSet<string> tagsForComponent = new();

            // tags
            if (item.Def.ComponentTags != null)
                tagsForComponent.UnionWith(
                    item.Def.ComponentTags.Select(i => i.Replace("{location}", location.ToString())));

            // id
            var identifier = item.ComponentDefID;
            tagsForComponent.Add(identifier);

            // category for component
            foreach (var component in item.GetComponents<Category>())
            {
                tagsForComponent.Add(component.CategoryID);
                if (!string.IsNullOrEmpty(component.Tag))
                    tagsForComponent.Add(component.Tag);
            }

            return tagsForComponent;
        }

        private void AddTagsToLocation(ChassisLocations location, HashSet<string> tags)
        {
            if (location == ChassisLocations.None) return;

            if (tagsOnLocations.TryGetValue(location, out var tagsOnLocation))
                tagsOnLocation.UnionWith(tags);
            else
                tagsOnLocations[location] = tags;
        }

        private bool AddError(string message)
        {
            if (errors == null)
            {
                error = message;
                return true;
            }

            errors[MechValidationType.InvalidInventorySlots].Add(new Text(message));

            return false;
        }

        private IEnumerable<string> RequiredTags(string tag)
        {
            if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

            if (restriction.RequiredTags == null) yield break;

            foreach (var requiredTag in restriction.RequiredTags) yield return requiredTag;
        }

        private IEnumerable<string> RequiredAnyTags(string tag)
        {
            if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

            if (restriction.RequiredAnyTags == null) yield break;

            foreach (var requiredTag in restriction.RequiredAnyTags) yield return requiredTag;
        }

        private IEnumerable<string> RequiredAnyTagsOnLocation(string tag)
        {
          if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

          if (restriction.RequiredAnyTagsOnSameLocation == null) yield break;

          foreach (var requredTag in restriction.RequiredAnyTagsOnSameLocation) yield return requredTag;
        }

        private IEnumerable<string> RequiredTagsOnSameLocation(string tag)
        {
            if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

            if (restriction.RequiredTagsOnSameLocation == null) yield break;

            foreach (var requiredTag in restriction.RequiredTagsOnSameLocation) yield return requiredTag;
        }

        private IEnumerable<string> IncompatibleTags(string tag)
        {
            if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

            if (restriction.IncompatibleTags == null) yield break;

            foreach (var incompatibleTag in restriction.IncompatibleTags) yield return incompatibleTag;
        }

        private IEnumerable<string> IncompatibleTagsOnSameLocation(string tag)
        {
            if (!TagRestrictionsHandler.Restrictions.TryGetValue(tag, out var restriction)) yield break;

            if (restriction.IncompatibleTagsOnSameLocation == null) yield break;

            foreach (var incompatibleTag in restriction.IncompatibleTagsOnSameLocation) yield return incompatibleTag;
        }

        private static string NameForTag(string tag)
        {
            {
                var categoryDescriptor = CategoryController.Shared.GetCategory(tag);
                if (categoryDescriptor != null) return categoryDescriptor._DisplayName;
            }

            {
                var dataManager = UnityGameInstance.BattleTechGame.DataManager;

                // ChassisDef

                if (dataManager.ChassisDefs.TryGet(tag, out var chassis)) return chassis.Description.UIName;

                // MechComponentDef

                if (dataManager.AmmoBoxDefs.TryGet(tag, out var ammoBox)) return ammoBox.Description.UIName;

                if (dataManager.HeatSinkDefs.TryGet(tag, out var heatSink)) return heatSink.Description.UIName;

                if (dataManager.JumpJetDefs.TryGet(tag, out var jumpJet)) return jumpJet.Description.UIName;

                if (dataManager.UpgradeDefs.TryGet(tag, out var upgrade)) return upgrade.Description.UIName;

                if (dataManager.WeaponDefs.TryGet(tag, out var weapon)) return weapon.Description.UIName;
            }

            return tag;
        }
    }
}