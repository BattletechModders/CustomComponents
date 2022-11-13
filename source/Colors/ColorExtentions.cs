using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using HBS;
using UnityEngine;

namespace CustomComponents
{
    public static class ColorExtentions
    {
        public static void SetCustomColor(this UIColorRefTracker color_tracker, UIColor uicolor, Color color)
        {
            color_tracker.SetUIColor(uicolor);
            if (uicolor == UIColor.Custom)
                color_tracker.OverrideWithColor(color);
        }

        public static void SetCustomColor(this IEnumerable<UIColorRefTracker> color_trackers, UIColor uicolor, Color color)
        {
            foreach (var color_tracker in color_trackers)
            {
                color_tracker.SetUIColor(uicolor);
                if (uicolor == UIColor.Custom)
                    color_tracker.OverrideWithColor(color);
            }
        }

        public static void SetUIColor(this UIColorRefTracker color_tracker, UIColor uicolor)
        {
            color_tracker.colorRef.UIColor = uicolor;
            color_tracker.colorRef.color = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.GetUIColor(uicolor);
        }

        public static void SetColor(this UIColorRefTracker color_tracker, MechComponentRef cref)
        {
            if (cref.Is<IColorComponent>(out var color))
                color_tracker.SetCustomColor(color.UIColor, color.RGBColor);
            else
                color_tracker.SetUIColor(MechComponentRef.GetUIColor(cref));
        }

        public static void SetColor(this UIColorRefTracker color_tracker, MechComponentDef cdef)
        {
            if (cdef.Is<IColorComponent>(out var color))
                color_tracker.SetCustomColor(color.UIColor, color.RGBColor);
            else
                color_tracker.SetUIColor(MechComponentDef.GetUIColor(cdef));
        }

        public static void SetColor(this IEnumerable<UIColorRefTracker> color_trackers, MechComponentRef cref)
        {
            if (cref.Is<IColorComponent>(out var color))
                color_trackers.SetCustomColor(color.UIColor, color.RGBColor);
            else
            {
                foreach (var color_tracker in color_trackers)
                    color_tracker.SetUIColor(MechComponentRef.GetUIColor(cref));
            }
        }

        public static void SetColor(this IEnumerable<UIColorRefTracker> color_trackers, MechComponentDef cdef)
        {
            if (cdef.Is<IColorComponent>(out var color))
                color_trackers.SetCustomColor(color.UIColor, color.RGBColor);
            else
            {
                foreach (var color_tracker in color_trackers)
                    color_tracker.SetUIColor(MechComponentDef.GetUIColor(cdef));
            }
        }

        public static void ChangeBackColor(MechComponentDef cdef, InventoryItemElement theWidget)
        {
            try
            {
                theWidget.iconBGColors.SetColor(cdef);
            }
            catch (Exception ex)
            {
                Logging.Error?.Log(ex);
            }
        }

        public static void ChangeBackColor(MechComponentDef cdef, InventoryItemElement_NotListView theWidget)
        {
            try
            {
                theWidget.iconBGColors.SetColor(cdef);
            }
            catch (Exception ex)
            {
                Logging.Error?.Log(ex);
            }
        }
    }
}