using SAIYA.Models;
using SAIYA.Systems;
using System.Drawing;
using System.Reflection;

namespace SAIYA.Content.Creatures
{
    public abstract class Creature
    {
        public virtual string Name { get; }
        public virtual string Description { get; } = "This creature hasn't been documented yet";
        public virtual string Requirements { get; } = "None";
        /// <summary>In seconds</summary>
        public abstract int HatchTime { get; }
        public virtual double Weight(User user) => 0;

        public string CreatureTexture = null;
        public string EggTexture = null;
        public Creature()
        {
            Name ??= GetType().Name;
            CreatureTexture = Directory.GetCurrentDirectory() + "\\Assets\\Creatures\\" + GetType().Name + ".png";
            EggTexture = Directory.GetCurrentDirectory() + "\\Assets\\Creatures\\" + GetType().Name + "Egg.png";
        }
    }
    public class Bleap : Creature
    {
        public override string Description => "A curious little slime often found in large groups. Sometimes they can be seen harassing lone gorbs. People describe them as pests and often use them as an alternative for footballs. Despite their rocky spines, they get a fair distance due to their elasticity.";
        public override int HatchTime => (int)TimeSpan.FromHours(1).TotalSeconds;
        public override double Weight(User user) => 1;
    }
    public class Daybreak : Creature
    {
        public override int HatchTime => (int)TimeSpan.FromHours(12).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(4, 7) ? 0.15 : 0;
        public override string Requirements => "Between 4am and 7am";
    }
    public class Etheria : Creature
    {
        public override string Description => "A mystical creature that is almost never sighted, let alone kept. It is theorised that they can phase through walls, hence their supernatural ability to evade capture.The only way to get one of these creatures is for it to imprint on you as a baby, but even then, finding one of their eggs can be near impossible.";
        public override string Requirements => "Between 6pm and 6am";
        public override int HatchTime => (int)TimeSpan.FromDays(3).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(18, 6) ? 0.05 : 0;
    }
    public class Eyezor : Creature
    {
        public override string Requirements => "Full moon";
        public override int HatchTime => (int)TimeSpan.FromDays(1).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(18, 6) && WeatherManager.CurrentMoonPhase == WeatherManager.MoonPhase.FullMoon ? 0.7 : 0;
    }
    public class Floofert : Creature
    {
        public override string Description => "Flooferts are very fluffy creatures, capable of surviving much harsher colds than a lot of the other creatures. Despite their sheep like characteristics, they rarely grow larger than a chihuahua due to their very slow aging. Flooferts can live upwards of 500 years, but people often mistake them as babies because of their size. If you pick up a floofert and look into its eyes, you can hear their thoughts. It's silence.";
        public override string Requirements => "Under 20C. More common under 10C";
        public override int HatchTime => (int)TimeSpan.FromHours(8).TotalSeconds;
        public override double Weight(User user) => WeatherManager.Temperature < 20 ? WeatherManager.Temperature < 10 ? 0.6 : 0.3 : 0;
    }
    public class Gleap : Creature
    {
        public override string Description => "Gleaps - short for  'golden bleaps'- are preposterously rare varients of bleaps.Not much is known about them, but legends describe their slimey residue acting as an infinite dessert of heavenly standards. Others say they bring good luck to the owner, while some legends say they drive their owners to insanity.Psychosis is a small price to pay for endless sugar.";
        public override int HatchTime => (int)TimeSpan.FromDays(7).TotalSeconds;
        public override double Weight(User user) => 0.01;
    }
    public class Gorb : Creature
    {
        public override string Description => "Gorbs are derpy little frogs that normally hunt emberflys. Due to their solitary nature, they are often preyed on by bleaps. The bleaps are actually herbiverous but are just bastards causing pain to lone gorbs. Gorbs have a tough slimey grey skin that is able to insulate them from immense heat, allowing them to explore the cavernous interior of volcanoes where emberfly nests are common. They burrow themselves at night, so they are never found outside of daytime.";
        public override string Requirements => "Between 8am-8pm. More common during rain";
        public override int HatchTime => (int)TimeSpan.FromHours(3).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(8, 20) ? WeatherManager.IsRaining ? 0.8 : 0.4 : 0;
    }
    public class Ignit : Creature
    {
        public override string Description => "Ignits are creatures that possess magical abilties. They use their magic in order to perform tricks, often trying to confuse visitors with seemingly simple, but normally impossible moves. Ignits favourite trick involve them hiding behind a rock until their audience comes to investigate, where they then disappear and reapear behind another rock. Their aura's have an almost time distorting effect- resulting in people getting trapped for hours playing with ignits, sometimes until they die of exhaustion They only come out when the sky is overcast";
        public override string Requirements => "Above 50% clouds coverage";
        public override int HatchTime => (int)TimeSpan.FromDays(1).TotalSeconds;
        public override double Weight(User user) => WeatherManager.Clouds > 0.5 ? 0.5 : 0;
    }
    public class Kroll : Creature
    {
        public override string Description => "Kroll's are likely the most feared creature. Capable of ripping prey apart cell by cell just by looking at them. What is even more worrying is their abundance during new moons. Their green glow inspires fear amongst everyone. Except gobbies. Gobbies aren't affected by Kroll's stare. Maybe their anatomy prevents it, or maybe they are too cute that even the Kroll doesn't want to kill them. Lucky for the rest of us, they dont seem to be seen outside of new moons.";
        public override string Requirements => "New moon";
        public override int HatchTime => (int)TimeSpan.FromDays(3).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(18, 6) && WeatherManager.CurrentMoonPhase == WeatherManager.MoonPhase.NewMoon ? 0.9 : 0;
    }
    public class Magmalean : Creature
    {
        public override string Requirements => "Above 30C";
        public override int HatchTime => (int)TimeSpan.FromHours(8).TotalSeconds;
        public override double Weight(User user) => WeatherManager.Temperature > 30 ? 0.4 : 0;
    }
    public class Midnight : Creature
    {
        public override string Description => "A shy creature that only appears around midnight. It's appetite is enormous and requires constant feeding. It also likes cooking its food with its flamelash tail.\nSome people believe that Midnights are a cousin of the Volcanine due to their similar flaming tails.";
        public override string Requirements => "Between 11pm and 1am";
        public override int HatchTime => (int)TimeSpan.FromDays(1).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(23, 1) ? 0.1 : 0;
    }
    public class Mooshie : Creature
    {
        public override string Description => "Mooshies are peculiar little fungus creatures that spontaneously appear during the rain. No one has ever seen a mooshie approach, they seemingly pop into existance when you arent looking. Because of their strange properties, some religious groups believe mooshies to be a path to the In Between- a reality between the dead and the alive. The devout believers often hunt mooshies and eat them. They are poisonous.";
        public override string Requirements => "Rain";
        public override int HatchTime => (int)TimeSpan.FromHours(5).TotalSeconds;
        public override double Weight(User user) => WeatherManager.IsRaining ? 0.6 : 0;
    }
    public class Obby : Creature
    {
        public override string Description => "Obbies are playful floating balls of obsidian. Resembling the personality of Golden Retrievers, obbies play fetch with loose stones and love getting pet on the underside of their orb. Their tongue is unfortnately coated in a weak acid, providing mild burns whenever they get too excited and decide they want to lick you.";
        public override int HatchTime => (int)TimeSpan.FromHours(2).TotalSeconds;
        public override double Weight(User user) => 0.6;
    }
    public class Obpod : Creature
    {
        public override string Requirements => "5pm-9pm. All day on Sundays";
        public override int HatchTime => (int)TimeSpan.FromHours(5).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.BetweenHours(17, 21) || Utilities.GetWATime.DayOfWeek == DayOfWeek.Sunday ? 0.4 : 0;
    }
    public class Pebleer : Creature
    {
        public override string Requirements => "Every Second Week";
        public override int HatchTime => (int)TimeSpan.FromHours(4).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.DayOfYear / 7 % 2 == 0 ? 0.3 : 0;
    }
    public class Pyra : Creature
    {
        public override string Description => "Pyras are the self-conscious cousins of bats. They are rather clumsy with their large flaming wings, often bumping into walls and proceeding to hurry off and hide out of embarassment. If you look at a pyra for too long, it may get angry and try to attack you. It wont succeed. Pyras tend to migrate away during Thursday to Sunday";
        public override string Requirements => "Monday-Wednesday";
        public override int HatchTime => (int)TimeSpan.FromHours(5).TotalSeconds;
        public override double Weight(User user) => Utilities.GetWATime.DayOfWeek >= DayOfWeek.Monday && Utilities.GetWATime.DayOfWeek <= DayOfWeek.Wednesday ? 0.4 : 0;
    }
    public class Ruby : Creature
    {
        public override string Description => "Rubies are aggressive miners with a diet of fresh stone. They are quite territorial and will attack anyone that comes near them when mining. They go on day long food trips every second day and cannot be found during these trips. However, they rest for the weekends and can be found before going off on Monday again";
        public override string Requirements => "Monday, Wednesday, Friday, Saturday, Sunday";
        public override int HatchTime => (int)TimeSpan.FromHours(5).TotalSeconds;
        public override double Weight(User user) => (int)Utilities.GetWATime.DayOfWeek % 2 == 0 || Utilities.GetWATime.DayOfWeek == DayOfWeek.Saturday ? 0.5 : 0;
    }
    public class Scorchfin : Creature
    {
        public override string Requirements => "Weekend, under 30C";
        public override int HatchTime => (int)TimeSpan.FromHours(6).TotalSeconds;
        public override double Weight(User user) => (Utilities.GetWATime.DayOfWeek == DayOfWeek.Saturday || Utilities.GetWATime.DayOfWeek == DayOfWeek.Sunday) && WeatherManager.Temperature < 30 ? 0.25 : 0;
    }
    public class Skylin : Creature
    {
        public override string Requirements => "Raining and above 20km/h winds";
        public override int HatchTime => (int)TimeSpan.FromHours(12).TotalSeconds;
        public override double Weight(User user) => WeatherManager.IsRaining && WeatherManager.WindSpeedKMH > 20 ? 0.6 : 0;
    }
    public class Smold : Creature
    {
        public override string Requirements => "Above 25km/h winds";
        public override int HatchTime => (int)TimeSpan.FromHours(12).TotalSeconds;
        public override double Weight(User user) => WeatherManager.WindSpeedKMH > 25 ? 0.4 : 0;
    }
    public class Thermaline : Creature
    {
        public override string Requirements => "Raining and above 15C";
        public override int HatchTime => (int)TimeSpan.FromHours(6).TotalSeconds;
        public override double Weight(User user) => WeatherManager.Temperature > 15 && WeatherManager.IsRaining ? 0.8 : 0;
    }
    public class Volcanine : Creature
    {
        public override string Description => "Volcanines are docile dog-like creatures that make their homes near volcano hotspots, preying on smaller creatures while leaving anything bigger than it alone. Volcanines are pack hunters and can be seen in groups of up to 15. Despite their large and intimidating figure, they love to play fetch, sometimes with unsuspecting obbys. They hate temperatures under 20C and will migrate to hotter climates until the temperature raises again.";
        public override string Requirements => "Weather above 20C";
        public override int HatchTime => (int)TimeSpan.FromHours(5).TotalSeconds;
        public override double Weight(User user) => WeatherManager.Temperature > 20 ? 0.5 : 0;
    }
}
