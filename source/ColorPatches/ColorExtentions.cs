using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public static class ColorExtentions
    {
        public static void SetColor(this UIColorRefTracker color_tracker, MechComponentRef cref)
        {
            if (cref.Is<IColorComponent>(out var color))
            {
                color_tracker.SetUIColor(color.UIColor);
                if (color.UIColor == UIColor.Custom)
                    color_tracker.OverrideWithColor(color.RGBColor);
            }
            else
            {
                color_tracker.SetUIColor(MechComponentRef.GetUIColor(cref));
            }
        }

        public static void SetColor(this UIColorRefTracker color_tracker, MechComponentDef cdef)
        {
            if (cdef.Is<IColorComponent>(out var color))
            {

                color_tracker.SetUIColor(color.UIColor);
                if (color.UIColor == UIColor.Custom)
                    color_tracker.OverrideWithColor(color.RGBColor);
            }
            else
            {
                color_tracker.SetUIColor(MechComponentDef.GetUIColor(cdef));
            }
        }

        public static void SetColor(this IEnumerable<UIColorRefTracker> color_trackers, MechComponentRef cref)
        {
            if (cref.Is<IColorComponent>(out var color))
            {
                foreach (var color_tracker in color_trackers)
                {
                    color_tracker.SetUIColor(color.UIColor);
                    if (color.UIColor == UIColor.Custom)
                        color_tracker.OverrideWithColor(color.RGBColor);

                }
            }
            else
            {
                foreach (var color_tracker in color_trackers)
                    color_tracker.SetUIColor(MechComponentRef.GetUIColor(cref));
            }
        }

        public static void SetColor(this IEnumerable<UIColorRefTracker> color_trackers, MechComponentDef cdef)
        {
            if (cdef.Is<IColorComponent>(out var color))
            {
                foreach (var color_tracker in color_trackers)
                {

                    color_tracker.SetUIColor(color.UIColor);
                    if (color.UIColor == UIColor.Custom)
                        color_tracker.OverrideWithColor(color.RGBColor);
                }
            }
            else
            {
                foreach (var color_tracker in color_trackers)
                    color_tracker.SetUIColor(MechComponentDef.GetUIColor(cdef));
            }
        }
    }
}