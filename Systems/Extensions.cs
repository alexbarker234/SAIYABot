using Pluralize.NET;
using System.Globalization;

namespace SAIYA.Systems
{
    public static class Extensions
    {
        public static string ToPing(this ulong obj) => $"<@{obj}>";
        public static int ToElapsedSeconds(this DateTime time) => (int)(time.Subtract(DateTime.UnixEpoch).TotalSeconds);
        public static bool BetweenHours(this DateTime time, int min, int max)
        {
            if (min > max) return time.Hour >= min || time.Hour < max;
            return time.Hour >= min && time.Hour < max; 
        }
        public static string ToTitleCase(this string title) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());

        public static IPluralize pluralizer = new Pluralizer();
        public static string Plural(this string text) => pluralizer.Pluralize(text);

        // RANDOM
        public static T Next<T>(this Random rand, List<T> list) => list[rand.Next(list.Count)];
        public static double NextDouble(this Random rand, double min, double max) => rand.NextDouble() * (max - min) + min;
    }
}
