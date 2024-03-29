﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BattleTech;
using BattleTech.UI;
using HBS.Collections;

namespace CustomComponents;

[HarmonyPatch(typeof(CombatHUD))]
[HarmonyPatch("Init")]
[HarmonyPatch(MethodType.Normal)]
[HarmonyPatch(new[] { typeof(CombatGameState) })]
public static class CombatHUD_InitTags {
  [HarmonyPrefix]
  [HarmonyWrapSafe]
  public static void Prefix(ref bool __runOriginal, CombatHUD __instance, CombatGameState Combat) {
    if (!__runOriginal)
    {
      return;
    }

    Log.Main.Info?.Log("Clearing tags cache");
    CustomCombatTagsHelper.ClearTagsCache();
  }
}
public static class CustomCombatTagsHelper {
  private static Dictionary<string, TagSet> tagsCache = new();
  private static readonly string CCComponentTagsStatName = "CCCombatComponentTags";
  private static readonly string CCComponentGUIDStatName = "CCComponentGUID";
  public static void ClearTagsCache() {
    tagsCache.Clear();
  }
  public static bool checkExistance(StatCollection statCollection, string statName) {
    return ((Dictionary<string, Statistic>)typeof(StatCollection).GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(statCollection)).ContainsKey(statName);
  }

  public static string getCCGUID(this MechComponent target) {
    string GUID;
    if (checkExistance(target.StatCollection, CCComponentGUIDStatName) == false) {
      GUID = Guid.NewGuid().ToString();
      target.StatCollection.AddStatistic(CCComponentGUIDStatName, GUID);
    } else {
      GUID = target.StatCollection.GetStatistic(CCComponentGUIDStatName).Value<string>();
    }
    return GUID;
  }
  private static TagSet prepareTags(MechComponent target) {
    //Control.Log($"Prepating tags for "+target.defId);
    var GUID = target.getCCGUID();
    TagSet tags = null;
    if (tagsCache.ContainsKey(GUID) == false) {
      //Control.Log($" not in cache");
      if (checkExistance(target.StatCollection, CCComponentTagsStatName) == false) {
        //Control.Log($" have no statistic value:"+ CustomCombatTagsHelper.CCComponentTagsStatName);
        tags = new();
        tags.AddRange(target.componentDef.ComponentTags);
      } else {
        var tags_string = target.StatCollection.GetStatistic(CCComponentTagsStatName).Value<string>();
        tags = TagSet.Parse(tags_string);
        //Control.Log($" have statistic value:" + CustomCombatTagsHelper.CCComponentTagsStatName+":"+tags_string);
      }
      tagsCache[GUID] = tags;
    } else {
      //Control.Log($" in cache");
      tags = tagsCache[GUID];
    }
    return tags;
  }
  private static void saveTags(MechComponent target,TagSet tags) {
    Log.Main.Info?.Log("saving tags "+target.defId+":"+tags);
    if (checkExistance(target.StatCollection, CCComponentTagsStatName) == false) {
      target.StatCollection.AddStatistic(CCComponentTagsStatName, tags.ToString());
    } else {
      target.StatCollection.Set(CCComponentTagsStatName, tags.ToString());
    }
  }
  public static TagSet ComponentTags(this MechComponent target) {
    var tags = prepareTags(target);
    if (tags == null) { tags = target.componentDef.ComponentTags; };
    return tags;
  }
  public static void AddTag(this MechComponent target, string tag) {
    var tags = prepareTags(target);
    if (tags == null) { return; }
    tags.Add(tag);
    saveTags(target,tags);
  }
  public static void AddTags(this MechComponent target, IEnumerable<string> itemsToAdd) {
    var tags = prepareTags(target);
    if (tags == null) { return; }
    tags.AddRange(itemsToAdd);
    saveTags(target, tags);
  }
  public static void RemoveTag(this MechComponent target, string tag) {
    var tags = prepareTags(target);
    if (tags == null) { return; }
    tags.Remove(tag);
    saveTags(target, tags);
  }
  public static void RemoveTags(this MechComponent target, IEnumerable<string> itemsToRemove) {
    var tags = prepareTags(target);
    if (tags == null) { return; }
    tags.RemoveRange(itemsToRemove);
    saveTags(target, tags);
  }
}