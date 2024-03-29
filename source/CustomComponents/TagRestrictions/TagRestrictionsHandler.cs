﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Localize;

namespace CustomComponents;

internal class TagRestrictionsHandler
{
    internal static TagRestrictionsHandler Shared = new();

    private Dictionary<string, TagRestrictions> _restrictions { get; set;  }
    internal static Dictionary<string, TagRestrictions> Restrictions => Shared._restrictions;

    internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
    {
        Log.CustomResource.Trace?.Log(" - TagRestriction");
        _restrictions = SettingsResourcesTools.Enumerate<TagRestrictions>("CCTagRestrictions", customResources)
            .ToDictionary(entry => entry.Tag);

        if (Log.CustomResource.Trace != null)
        {
            foreach (var pair in _restrictions)
            {
                Log.CustomResource.Trace.Log($" -- {pair.Key}");
            }
        }
    }

    internal bool ValidateMechCanBeFielded(MechDef mechDef)
    {
        var checker = new TagsChecker(mechDef);
        return checker.Validate() == null;
    }

    internal void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
    {
        var checker = new TagsChecker(mechDef);
        checker.Validate(errors);
    }

    public string ValidateDrop(MechLabItemSlotElement drop_item, ChassisLocations location)
    {
        var checker = new TagsChecker(MechLabHelper.CurrentMechLab.ActiveMech);
        return checker.ValidateDrop(
            Control.Settings.TagRestrictionDropValidateRequiredTags,
            Control.Settings.TagRestrictionDropValidateIncompatibleTags,
            drop_item.ComponentRef,
            location
        );
    }
}