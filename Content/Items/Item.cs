using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA.Content.Items
{
    public abstract class Item
    {
        public virtual string Name { get; }
        public virtual string Description { get; } = "This item hasn't been documented yet";
        public virtual int Price { get; } = -1;
        public virtual bool Buyable { get; } = true;
        public virtual bool Sellable { get; } = true;
        public virtual ItemTag Tag { get; } = ItemTag.Misc;

        public Item()
        {
            Name ??= GetType().Name;
        }
    }
    public enum ItemTag
    {
        Misc,
        Fish,
        Seed,
        Plant,
    }
}
