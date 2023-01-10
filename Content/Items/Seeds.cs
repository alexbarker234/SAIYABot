using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA.Content.Items
{
    public abstract class SeedItem : Item
    {
        public sealed override ItemTag Tag => ItemTag.Seed;
    }
    public class SparkweedSeeds : SeedItem
    {
        public override string Name => "Sparkweed Seeds";
        public override int Price => 100;
    }
    public class CoalsproutSeeds : SeedItem
    {
        public override string Name => "Coalsprout Seeds";
        public override int Price => 100;
    }
    public class ChardaisySeeds : SeedItem
    {
        public override string Name => "Chardaisy Seeds";
        public override int Price => 150;
    }
    public class SearcapSeeds : SeedItem
    {
        public override string Name => "Searcap Seeds";
        public override int Price => 200;
    }
    public class GasbloomSeeds : SeedItem
    {
        public override string Name => "Gasbloom Seeds";
        public override int Price => 500;
    }
    public class StarlightSpudSeeds : SeedItem
    {
        public override string Name => "Starlight Spud Seeds";
        public override int Price => 250;
        public override bool Buyable => false;
    }
    public class ScorchbeanSeeds : SeedItem
    {
        public override string Name => "Scorchbean Seeds";
        public override int Price => 500;
        public override bool Buyable => false;
    }
    public class SparklethornSeeds : SeedItem
    {
        public override string Name => "Sparklethorn Seeds";
        public override int Price => 600;
        public override bool Buyable => false;
    }
    public class AshdrakeSeeds : SeedItem
    {
        public override string Name => "Ashdrake Seeds";
        public override int Price => 700;
        public override bool Buyable => false;
    }
    public class StarbellSeeds : SeedItem
    {
        public override string Name => "Starbell Seeds";
        public override int Price => 5000;
        public override bool Buyable => false;
    }
}
