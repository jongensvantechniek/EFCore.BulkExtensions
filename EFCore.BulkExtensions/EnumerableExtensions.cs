using System.Collections.Generic;
using System.Linq;

namespace EFCore.BulkExtensions
{
    public static class EnumerableExtensions
    {
        public static T Head<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.FirstOrDefault();
        }

        public static IEnumerable<T> Tail<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Skip(1);
        }

        public static IList<T> OrderByProperties<T>(this IEnumerable<T> enumerable,
            IEnumerable<string> propertyNames)
        {
            if (enumerable.Count() <= 1 || !propertyNames.Any())
            {
                return enumerable.ToList();
            }

            var ordered = enumerable.OrderBy(s => typeof(T).GetProperty(propertyNames.Head()).GetValue(s));
            return propertyNames.Count() == 1 ? ordered.ToList() : ordered.ThenByProperties(propertyNames.Tail());
        }

        public static IList<T> ThenByProperties<T>(this IOrderedEnumerable<T> enumerable,
            IEnumerable<string> propertyNames)
        {
            if (enumerable.Count() <= 1 || !propertyNames.Any())
            {
                return enumerable.ToList();
            }

            var ordered = enumerable.ThenBy(s => typeof(T).GetProperty(propertyNames.Head()).GetValue(s));
            return propertyNames.Count() == 1 ? ordered.ToList() : ordered.ThenByProperties(propertyNames.Tail());
        }
    }
}