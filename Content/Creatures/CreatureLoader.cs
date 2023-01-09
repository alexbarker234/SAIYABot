using System.Reflection;

namespace SAIYA.Content.Creatures
{
    public static class CreatureLoader
    {
        public static Dictionary<string, Creature> creatures = new();
        public static void Load()
        {
            creatures = new();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(Creature)))
                {
                    var creature = (Creature)Activator.CreateInstance(type, null);
                    creatures.Add(creature.Name, creature);
                }
            }
        }
    }
}
