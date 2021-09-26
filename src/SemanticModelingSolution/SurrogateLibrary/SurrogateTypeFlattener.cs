using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    //public static class SurrogateTypeFlattener
    //{
    //    public static IList<NavigationArc> FlatHierarchyProperties(this SurrogateType node)
    //    {
    //        Dictionary<string, SurrogateType> visited = new();
    //        var result = FlatHierarchyPropertiesInternal(node, null, visited).ToList();
    //        foreach (var item in result)
    //        {
    //            SurrogateProperty current = null;
    //            StringBuilder sb = new();
    //            do
    //            {
    //                if(current != null) sb.Append(".");
    //                current = item.Next.;
    //                sb.Append(current.Name);
    //            }
    //            while (current != null);

    //            item.Path = sb.ToString();
    //        }

    //        return result;
    //    }

    //    private static IEnumerable<NavigationArc> FlatHierarchyPropertiesInternal(SurrogateType type,
    //        NavigationArc previous, Dictionary<string, SurrogateType> visited)
    //    {
    //        if (!visited.TryGetValue(type.FullName, out _))
    //        {
    //            visited[type.FullName] = type;
    //            foreach (var property in type.Properties.Values)
    //            {
    //                var navigation = new NavigationArc(previous, property);
    //                if (property.PropertyType.IsBasicType)
    //                {
    //                    yield return navigation;
    //                    continue;
    //                }

    //                SurrogateType next;
    //                if (property.PropertyType.IsCollection())
    //                    next = property.PropertyType.InnerType1;
    //                else
    //                    next = property.PropertyType;

    //                foreach (var sub in FlatHierarchyPropertiesInternal(next, navigation, visited))
    //                {
    //                    yield return sub;
    //                }
    //            }
    //        }
    //    }

    //}
    public static class SurrogateTypeFlattener2
    {
        public static IList<NavigationProperty> FlatHierarchyProperties(this SurrogateType type)
        {
            List<NavigationProperty> result = new();
            var nav = new NavigationProperty(null, type);
            Descend(result, nav, type);

            return result;
        }

        private static bool Descend(List<NavigationProperty> result, NavigationProperty nav, SurrogateType type)
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

                var next = new NavigationProperty(nav, property);
                nav.SetNext(next);
                nav = next;

                SurrogateType descendType;
                if (property.PropertyType.IsCollection())
                {
                    descendType = property.PropertyType.InnerType1;

                    var next2 = new NavigationProperty(next, descendType);
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



        //private static IEnumerable<NavigationArc2> FlatHierarchyPropertiesInternal(SurrogateProperty previous,
        //    SurrogateType type, Dictionary<UInt64, SurrogateProperty> visited, string path)
        //{
        //    if (previous == null || !visited.TryGetValue(previous.Index, out _))
        //    {
        //        if(previous != null) visited[previous.Index] = previous;

        //        path += ".";
        //        foreach (var property in type.Properties.Values)
        //        {
        //            var navigation = new NavigationArc2(previous, property);
        //            var innerPath = path + property.Name;
        //            if (property.PropertyType.IsBasicType)
        //            {
        //                navigation.Path = innerPath;
        //                yield return navigation;
        //                continue;
        //            }

        //            SurrogateType next;
        //            if (property.PropertyType.IsCollection())
        //            {
        //                next = property.PropertyType.InnerType1;
        //                innerPath += $".{next.Name}";
        //            }
        //            else
        //            {
        //                next = property.PropertyType;
        //            }

        //            foreach (var sub in FlatHierarchyPropertiesInternal(property, next, visited, innerPath))
        //            {
        //                yield return sub;
        //            }
        //        }
        //    }
        //}
    }

}
