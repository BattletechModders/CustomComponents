using System.Linq;
using BattleTech;

namespace CustomComponents
{
    public static class CategoriesExtentions
    {
        public static bool IsCategory(this MechComponentDef cdef, string category)
        {
            return cdef.GetComponents<Category>().Any(c => c.CategoryID == category);

            // Is<Category>
            // GetComponent<Category>
        }

        public static bool IsCategory(this MechComponentRef cref, string category)
        {
            return cref.GetComponents<Category>().Any(c => c.CategoryID == category);
        }
        public static bool IsCategory(this BaseComponentRef cref, string category)
        {
            return cref.GetComponents<Category>().Any(c => c.CategoryID == category);
        }


        public static bool IsCategory(this MechComponentDef cdef, string categoryid, out Category category)
        {
            category = cdef.GetComponents<Category>().FirstOrDefault(c => c.CategoryID == categoryid);
            return category != null;
            // Is<Category>
            // GetComponent<Category>
        }

        public static bool IsCategory(this MechComponentRef cref, string categoryid, out Category category)
        {
            category = cref.GetComponents<Category>().FirstOrDefault(c => c.CategoryID == categoryid);
            return category != null;
        }

        public static bool IsCategory(this BaseComponentRef cref, string categoryid, out Category category)
        {
            category = cref.GetComponents<Category>().FirstOrDefault(c => c.CategoryID == categoryid);
            return category != null;
        }

        

    }
}