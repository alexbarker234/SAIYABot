using SAIYA.Content.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA.Content.Plants
{
    public abstract class Plant : Item
    {
        public override string Description { get; } = "This plant hasn't been documented yet";
        public virtual string PlantedEffect { get; }
        public abstract TimeSpan GrowTime { get; }
        public abstract TimeSpan WaterRate { get; }
        public abstract int Yield { get; }
        public sealed override ItemTag Tag => ItemTag.Plant;
        public sealed override bool Buyable => false;
        public string PlantTexture = null;
        public Plant()
        {
            PlantTexture = Directory.GetCurrentDirectory() + "\\Assets\\Garden\\Plants\\" + GetType().Name + ".png";
        }
    }
    public class Sparkweed : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(1);
        public override TimeSpan WaterRate => TimeSpan.FromDays(1);
        public override int Yield => 10;
        public override int Price => 30;
    }
    public class Coalsprout : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(1);
        public override TimeSpan WaterRate => TimeSpan.FromDays(1);
        public override int Yield => 10;
        public override int Price => 30;
    }
    public class Chardaisy : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(2);
        public override TimeSpan WaterRate => TimeSpan.FromDays(1);
        public override int Yield => 10;
        public override int Price => 60;
    }
    public class Searcap : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(3);
        public override TimeSpan WaterRate => TimeSpan.FromHours(18);
        public override int Yield => 15;
        public override int Price => 60;
    }
    public class Gasbloom : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(3);
        public override TimeSpan WaterRate => TimeSpan.FromHours(12);
        public override int Yield => 30;
        public override int Price => 40;
    }
    public class StarlightSpud : Plant
    {
        public override string Name => "Starlight Spud";
        public override TimeSpan GrowTime => TimeSpan.FromDays(7);
        public override TimeSpan WaterRate => TimeSpan.FromDays(2);
        public override int Yield => 10;
        public override int Price => 100;
    }
    public class Scorchbean : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(3);
        public override TimeSpan WaterRate => TimeSpan.FromHours(8);
        public override int Yield => 50;
        public override int Price => 25;
    }
    public class Sparklethorn : Plant
    {
        public override TimeSpan GrowTime => TimeSpan.FromDays(5);
        public override TimeSpan WaterRate => TimeSpan.FromHours(10);
        public override int Yield => 25;
        public override int Price => 50;
    }
    public class Ashdrake : Plant
    {
        public override string Description => "A sentient plant used in very powerful magical potions. However, it sucks water from neighbouring plants and one plant does not yield very many Ashdrakes";
        public override string PlantedEffect => "Increases all plants water need by 20%";
        public override TimeSpan GrowTime => TimeSpan.FromDays(5);
        public override TimeSpan WaterRate => TimeSpan.FromHours(20);
        public override int Yield => 3;
        public override int Price => 500;
    }
    public class Starbell : Plant
    {
        public override string Description => "A very juicy fruit that grows in tropical climates. It is not commonly found in the Ember Rift due to its dryness and is often imported.";
        public override string PlantedEffect => "Decreases water need for all plants by 10%. **HOWEVER** planting more than one Starbell will increase water rate drastically";
        public override TimeSpan GrowTime => TimeSpan.FromDays(10);
        public override TimeSpan WaterRate => TimeSpan.FromHours(10);
        public override int Yield => 25;
        public override int Price => 300;
    }
}
