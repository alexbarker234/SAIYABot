using SAIYA.Content.Plants;
using System.Reflection;

namespace SAIYA.Content.Items
{
    public static class ItemLoader
    {
        public static Dictionary<string, Item> items = new();
        public static Dictionary<string, Plant> plants = new();
        public static Dictionary<string, Fish> fish = new();
        public static void Load()
        {
            items = new();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(Item)))
                {
                    var item = (Item)Activator.CreateInstance(type, null);
                    items.Add(item.Name, item);

                    if (type.IsSubclassOf(typeof(Fish)))
                        fish.Add(item.Name, item as Fish);
                    else if (type.IsSubclassOf(typeof(Plant)))
                        plants.Add(item.Name, item as Plant);
                }
            }
        }
    }
}
