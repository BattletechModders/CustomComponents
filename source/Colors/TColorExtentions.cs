using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using UnityEngine;

namespace CustomComponents
{
    public static class TColorExtentions
    {
        //[Conditional("COLORICON")]
        public static void SetTColor(this UIColorRefTracker color_text, UIColorRefTracker color_icon, MechComponentRef cref)
        {
            Control.LogDebug(DType.Color, $"TextColor for {cref.ComponentDefID}");
            if (cref.Is<ITColorComponent>(out var color))
            {
                Control.LogDebug(DType.Color, $"-- color found set to {color.UIColor}/{color.RGBColor}");

                if(color.SkipText)
                    color_text.SetUIColor(UIColor.White);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                         color_icon.SetCustomColor(color.UIColor, color.RGBColor);
           }
            else
            {
                Control.LogDebug(DType.Color, $"-- no color set to white");
                color_text.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        //[Conditional("COLORICON")]
        public static void SetTColor(this UIColorRefTracker color_text, UIColorRefTracker color_icon, MechComponentDef cdef)
        {
            if (cdef.Is<ITColorComponent>(out var color))
            {
                if (color.SkipText)
                    color_text.SetUIColor(UIColor.White);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        //[Conditional("COLORICON")]
        public static void SetTColor(this IEnumerable<UIColorRefTracker> color_text, UIColorRefTracker color_icon, MechComponentRef cref)
        {
            if (cref.Is<ITColorComponent>(out var color))
            {
                if (color.SkipText)
                    color_text.SetCustomColor(UIColor.White, Color.white);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
            }
            else
            {
                foreach (var color_tracker in color_text)
                    color_tracker.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        //[Conditional("COLORICON")]
        public static void SetTColor(this IEnumerable<UIColorRefTracker> color_text, UIColorRefTracker color_icon, MechComponentDef cdef)
        {
            if (cdef.Is<ITColorComponent>(out var color))
            {
                if (color.SkipText)
                    color_text.SetCustomColor(UIColor.White, Color.white);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
            }
            else
            {
                foreach (var color_tracker in color_text)
                    color_tracker.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        //[Conditional("COLORICON")]
        internal static void ChangeTextIconColor(MechComponentDef cdef, InventoryItemElement_NotListView theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon == null)
            {
                color_icon = theWidget.icon.gameObject.AddComponent<UIColorRefTracker>();
            }

            if (cdef.Is<ITColorComponent>(out var color))
            {
                if (color.SkipText)
                    color_text.SetCustomColor(UIColor.White, Color.white);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                color_icon.SetUIColor(UIColor.White);
            }
        }


        //[Conditional("COLORICON")]
        public static void ChangeTextIconColor(MechComponentDef cdef, InventoryItemElement theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon == null)
            {
                color_icon = theWidget.icon.gameObject.AddComponent<UIColorRefTracker>();
            }

            if (cdef.Is<ITColorComponent>(out var color))
            {
                if (color.SkipText)
                    color_text.SetCustomColor(UIColor.White, Color.white);
                else
                    color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.SkipIcon)
                        color_icon.SetUIColor(UIColor.White);
                    else
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                color_icon.SetUIColor(UIColor.White);
            }
        }

        //[Conditional("COLORICON")]
        public static void ResetTextIconColor(InventoryItemElement theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon != null)
            {
                color_icon.SetUIColor(UIColor.White);
            }
            color_text.SetUIColor(UIColor.White);
        }

        //[Conditional("COLORICON")]
        internal static void ResetTextIconColor(InventoryItemElement_NotListView theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon != null)
            {
                color_icon.SetUIColor(UIColor.White);
            }
            color_text.SetUIColor(UIColor.White);
        }

    }
}