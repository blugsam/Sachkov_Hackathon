using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachkov_Hackathon
{
    public enum ItemStatus
    {
        Received,
        Shelving,
        Shelved,
        Picking,
        Picked,
        Shipped
    }

    public class Item : IComparable<Item>, IEquatable<Item>
    {
        public string Name { get; init; }
        public int Id { get; init; }
        public string Type { get; init; }
        public int Price { get; init; }
        public ItemStatus Status { get; set; }

        public Item(string name, int id, string type, int price)
        {
            Name = name;
            Id = id;
            Type = type;
            Price = price;
        }

        public int CompareTo(Item other)
        {
            if (other == null) return 1;
            return Id.CompareTo(other.Id);
        }

        public bool Equals(Item other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as Item);

        public override int GetHashCode() => Id.GetHashCode();

    }

}