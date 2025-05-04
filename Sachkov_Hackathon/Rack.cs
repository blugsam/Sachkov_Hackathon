using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachkov_Hackathon
{
    public class Rack : IComparable<Rack>
    {
        public int Id { get; init; }
        private readonly List<Item> _items;
        private const int capacity = 10;
        private readonly object _lock = new object();

        public Rack(int id)
        {
            Id = id;
            _items = new List<Item>();
        }

        public bool PutItem(Item item)
        {
            lock (_lock)
            {
                if (_items.Count < capacity)
                {
                    _items.Add(item);
                    return true;
                }
            }

            return false;
        }

        public Item GetItem()
        {
            Item item = _items[_items.Count - 1];

            return item;
        }

        public Item? GetItem(int id)
        {
            var item = _items.FirstOrDefault(p => p.Id == id);

            return item;
        }

        public Item? RemoveItem(int id)
        {
            lock (_lock)
            {
                Item? removedItem = null;
                foreach (var item in _items)
                {
                    if (item.Id == id)
                    {
                        removedItem = item;
                        _items.Remove(item);
                        break;
                    }
                }
                return removedItem;
            }
        }

        public List<Item> GetItems()
        {
            return _items.Select(i => new Item(i.Name, i.Id, i.Type, i.Price)).ToList();
        }


        public int CompareTo(Rack other)
        {
            if (other == null) return 1;
            return Id.CompareTo(other.Id);
        }

        private bool TryFindItemById(int articleId, out Item item)
        {
            item = _items.FirstOrDefault(p => p.Id == articleId);
            return item != null;
        }
    }
}