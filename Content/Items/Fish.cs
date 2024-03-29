﻿using SAIYA.Models;

namespace SAIYA.Content.Items
{
    public abstract class Fish : Item
    {
        public sealed override ItemTag Tag => ItemTag.Fish;
        public virtual double Weight(User user) => 0;
        public sealed override bool Buyable => false;
    }

    // CONTENT
    public class Goldie : Fish
    {
        public override int Price => 10;
        public override double Weight(User user) => 1;
    }
    public class Trifin : Fish
    {
        public override int Price => 10;
        public override double Weight(User user) => 1;
    }
    public class Jadefin : Fish
    {
        public override int Price => 10;
        public override double Weight(User user) => 1;
    }
    public class Stripe : Fish
    {
        public override int Price => 20;
        public override double Weight(User user) => 1;
    }
    public class Redfin : Fish
    {
        public override int Price => 20;
        public override double Weight(User user) => 0.7;
    }
    public class Ashjelly : Fish
    {
        public override int Price => 10;
        public override double Weight(User user) => 0.5;
    }

    public class Darkray : Fish
    {
        public override int Price => 50;
        public override double Weight(User user) => 0.5;
    }
    public class Inky : Fish
    {
        public override int Price => 30;
        public override double Weight(User user) => 0.5;
    }

    public class Deepjaw : Fish
    {
        public override int Price => 100;
        public override double Weight(User user) => 0.3;
    }
    public class Bloodgill : Fish
    {
        public override int Price => 100;
        public override double Weight(User user) => 0.3;
    }
    public class Emberfin : Fish
    {
        public override int Price => 100;
        public override double Weight(User user) => 0.3;
    }
    public class Toxeel : Fish
    {
        public override int Price => 500;
        public override double Weight(User user) => 0.1;
    }
}
