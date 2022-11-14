using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomComponents {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_InitTags {
    public static bool Prefix(CombatHUD __instance, CombatGameState Combat) {
      Log.Main.Info?.Log("Clearing tags cache");
      CustomCombatTagsHelper.ClearTagsCache();
      return true;
    }
  }
  public static class CustomCombatTagsHelper {
    private static Dictionary<string, TagSet> tagsCache = new Dictionary<string, TagSet>();
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
        target.StatCollection.AddStatistic<string>(CCComponentGUIDStatName, GUID);
      } else {
        GUID = target.StatCollection.GetStatistic(CCComponentGUIDStatName).Value<string>();
      }
      return GUID;
    }
    private static TagSet prepareTags(MechComponent target) {
      //Control.Log($"Prepating tags for "+target.defId);
      string GUID = target.getCCGUID();
      TagSet tags = null;
      if (tagsCache.ContainsKey(GUID) == false) {
        //Control.Log($" not in cache");
        if (checkExistance(target.StatCollection, CCComponentTagsStatName) == false) {
          //Control.Log($" have no statistic value:"+ CustomCombatTagsHelper.CCComponentTagsStatName);
          tags = new TagSet();
          tags.AddRange(target.componentDef.ComponentTags);
        } else {
          string tags_string = target.StatCollection.GetStatistic(CCComponentTagsStatName).Value<string>();
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
      Log.Main.Info?.Log($"saving tags "+target.defId+":"+tags);
      if (checkExistance(target.StatCollection, CCComponentTagsStatName) == false) {
        target.StatCollection.AddStatistic<string>(CCComponentTagsStatName, tags.ToString());
      } else {
        target.StatCollection.Set<string>(CCComponentTagsStatName, tags.ToString());
      }
    }
    public static TagSet ComponentTags(this MechComponent target) {
      TagSet tags = prepareTags(target);
      if (tags == null) { tags = target.componentDef.ComponentTags; };
      return tags;
    }
    public static void AddTag(this MechComponent target, string tag) {
      TagSet tags = prepareTags(target);
      if (tags == null) { return; }
      tags.Add(tag);
      saveTags(target,tags);
    }
    public static void AddTags(this MechComponent target, IEnumerable<string> itemsToAdd) {
      TagSet tags = prepareTags(target);
      if (tags == null) { return; }
      tags.AddRange(itemsToAdd);
      saveTags(target, tags);
    }
    public static void RemoveTag(this MechComponent target, string tag) {
      TagSet tags = prepareTags(target);
      if (tags == null) { return; }
      tags.Remove(tag);
      saveTags(target, tags);
    }
    public static void RemoveTags(this MechComponent target, IEnumerable<string> itemsToRemove) {
      TagSet tags = prepareTags(target);
      if (tags == null) { return; }
      tags.RemoveRange(itemsToRemove);
      saveTags(target, tags);
    }
  }
}
