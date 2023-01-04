using SAIYA.Models;
using System.Drawing;
using System.Reflection;

namespace SAIYA.Creatures
{
    public static class CreatureLoader
    {
        public static List<Creature> creatures = new();
        public static void Load()
        {
            creatures = new();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(Creature)))
                {
                    var creature = (Creature)Activator.CreateInstance(type, null);
                    creatures.Add(creature);
                }
            }       
        }
    }
    public abstract class Creature
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual string Requirements { get; } = "None";
        /// <summary>In seconds</summary>
        public abstract int HatchTime { get; }
        public virtual double Weight(User user) => 0;

        public string CreatureTexture = null;
        public string EggTexture = null;
        public Creature()
        {
            CreatureTexture = Directory.GetCurrentDirectory() + "\\Assets\\Creatures\\" + GetType().Name + ".png";
            EggTexture = Directory.GetCurrentDirectory() + "\\Assets\\Creatures\\" + GetType().Name + "Egg.png";
        }
    }
    public class Etheria : Creature
    {
        public override string Name => "Etheria";
        public override string Description => "A mystical creature that is almost never sighted, let alone kept. It is theorised that they can phase through walls, hence their supernatural ability to evade capture.The only way to get one of these creatures is for it to imprint on you as a baby, but even then, finding one of their eggs can be near impossible.";
        public override int HatchTime => 3 * 24 * 60 * 60;
        public override double Weight(User user) => 0.1;
    }
    public class Bleap : Creature
    {
        public override string Name => "Bleap";
        public override string Description => "A curious little slime often found in large groups. Sometimes they can be seen harassing lone gorbs. People describe them as pests and often use them as an alternative for footballs. Despite their rocky spines, they get a fair distance due to their elasticity.";
        public override int HatchTime => 1 * 60 * 60;
        public override double Weight(User user) => 1;
    }
}
