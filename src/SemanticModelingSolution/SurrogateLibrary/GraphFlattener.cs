using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class GraphFlattener
    {
        public static IList<NavigationPath> FlattenHierarchy(this SurrogateType type)
        {
            List<NavigationPath> result = new();
            var nav = new NavigationPath(null, type);
            Descend(result, nav, type);

            return result;
        }

        private static bool Descend(List<NavigationPath> result, NavigationPath nav, SurrogateType type)
        {
            var res = false;
            foreach (var propInfo in type.Properties)
            {
                res = true;
                var property = propInfo.Value;

                if (nav.PointsBack(property))
                {
                    // cycle detected

                    // these two lines are useful only when debugging
                    //nav.UpdateCache();
                    //Console.WriteLine($"{nav} - Discarded {property}");

                    continue;
                }

                var next = new NavigationPath(nav, property);
                nav.SetNext(next);
                nav = next;

                SurrogateType descendType;
                if (property.PropertyType.IsCollection())
                {
                    descendType = property.PropertyType.InnerType1;

                    var next2 = new NavigationPath(next, descendType);
                    next.SetNext(next2);
                    nav = next2;
                }
                else
                {
                    descendType = property.PropertyType;
                }

                bool found = Descend(result, nav, descendType);

                if (!found)
                {
                    var cloned = nav.CloneRoot();

                    //these lines are useful only when debugging
                    //List<NavigationProperty> items = new();
                    //var temp = nav;
                    //while (temp != null)
                    //{
                    //    items.Insert(0, temp with { });
                    //    temp = temp.Previous;
                    //}

                    //var paths = items.Select(s => s.Property == null ? s.Type.Name : s.Property.Name);
                    //Console.WriteLine($"{string.Join(".", paths)}");
                    //cloned.UpdateCache();
                    result.Add(cloned.GetRoot());
                }

                nav = nav.Previous;
                nav.SetNext(null);

                // this line is useful only when debugging
                //nav.UpdateCache();
            }

            return res;
        }

    }

}
