using SAIYA.Creatures;
using System.Reflection;

namespace SAIYA.Items
{
    public static class ItemLoader
    {
        public static List<Item> items = new();
        public static void Load()
        {
            items = new();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(Item)))
                {
                    var item = (Item)Activator.CreateInstance(type, null);
                    items.Add(item);

                    if (type.IsSubclassOf(typeof(Fish)))
                        FishLoader.fish.Add(item as Fish);
                }
            }
        }
    }
    public abstract class Item
    {
        public virtual string Name { get; }
        public virtual string Description { get; } = "This item hasn't been documented yet";
        public virtual int Price { get; } = 0;
        public Item()
        {
            if (Name == null) Name = GetType().Name;
        }
    }

}
