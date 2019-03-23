using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomComponents {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_InitTags {
    public static bool Prefix(CombatHUD __instance, CombatGameState Combat) {
      Control.Log("Clearing tags cache");
      CustomCombatTagsHelper.ClearTagsCache();
      return true;
    }
  }
  public static class CustomCombatTagsHelper {
    private static Dictionary<string, TagSet> tagsCache = new Dictionary<string, TagSet>();
    private static readonly string CCComponentTagsStatName = "CCCombatComponentTags";
    private static readonly string CCComponentGUIDStatName = "CCComponentGUID";
    public static void ClearTagsCache() {
      CustomCombatTagsHelper.tagsCache.Clear();
    }
    public static bool checkExistance(StatCollection statCollection, string statName) {
      return ((Dictionary<string, Statistic>)typeof(StatCollection).GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(statCollection)).ContainsKey(statName);
    }

    public static string getCCGUID(this MechComponent target) {
      string GUID;
      if (CustomCombatTagsHelper.checkExistance(target.StatCollection, CustomCombatTagsHelper.CCComponentGUIDStatName) == false) {
        GUID = System.Guid.NewGuid().ToString();
        target.StatCollection.AddStatistic<string>(CustomCombatTagsHelper.CCComponentGUIDStatName, GUID);
      } else {
        GUID = target.StatCollection.GetStatistic(CustomCombatTagsHelper.CCComponentGUIDStatName).Value<string>();
      }
      return GUID;
    }
    private static TagSet prepareTags(MechComponent target) {
      Control.Log($"Prepating tags for "+target.defId);
      string GUID = target.getCCGUID();
      TagSet tags = null;
      if (CustomCombatTagsHelper.tagsCache.ContainsKey(GUID) == false) {
        Control.Log($" not in cache");
        if (CustomCombatTagsHelper.checkExistance(target.StatCollection, CustomCombatTagsHelper.CCComponentTagsStatName) == false) {
          Control.Log($" have no statistic value:"+ CustomCombatTagsHelper.CCComponentTagsStatName);
          tags = new TagSet();
          tags.AddRange(target.componentDef.ComponentTags);
        } else {
          string tags_string = target.StatCollection.GetStatistic(CustomCombatTagsHelper.CCComponentTagsStatName).Value<string>();
          tags = TagSet.Parse(tags_string);
          Control.Log($" have statistic value:" + CustomCombatTagsHelper.CCComponentTagsStatName+":"+tags_string);
        }
        CustomCombatTagsHelper.tagsCache[GUID] = tags;
      } else {
        Control.Log($" in cache");
        tags = CustomCombatTagsHelper.tagsCache[GUID];
      }
      return tags;
    }
    private static void saveTags(MechComponent target,TagSet tags) {
      Control.Log($"saving tags "+target.defId+":"+tags.ToString());
      if (CustomCombatTagsHelper.checkExistance(target.StatCollection, CustomCombatTagsHelper.CCComponentTagsStatName) == false) {
        target.StatCollection.AddStatistic<string>(CustomCombatTagsHelper.CCComponentTagsStatName, tags.ToString());
      } else {
        target.StatCollection.Set<string>(CustomCombatTagsHelper.CCComponentTagsStatName, tags.ToString());
      }
    }
    public static TagSet ComponentTags(this MechComponent target) {
      TagSet tags = CustomCombatTagsHelper.prepareTags(target);
      if (tags == null) { tags = target.componentDef.ComponentTags; };
      return tags;
    }
    public static void AddTag(this MechComponent target, string tag) {
      TagSet tags = CustomCombatTagsHelper.prepareTags(target);
      if (tags == null) { return; }
      tags.Add(tag);
      CustomCombatTagsHelper.saveTags(target,tags);
    }
    public static void AddTags(this MechComponent target, IEnumerable<string> itemsToAdd) {
      TagSet tags = CustomCombatTagsHelper.prepareTags(target);
      if (tags == null) { return; }
      tags.AddRange(itemsToAdd);
      CustomCombatTagsHelper.saveTags(target, tags);
    }
    public static void RemoveTag(this MechComponent target, string tag) {
      TagSet tags = CustomCombatTagsHelper.prepareTags(target);
      if (tags == null) { return; }
      tags.Remove(tag);
      CustomCombatTagsHelper.saveTags(target, tags);
    }
    public static void RemoveTags(this MechComponent target, IEnumerable<string> itemsToRemove) {
      TagSet tags = CustomCombatTagsHelper.prepareTags(target);
      if (tags == null) { return; }
      tags.RemoveRange(itemsToRemove);
      CustomCombatTagsHelper.saveTags(target, tags);
    }
  }
}
