using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Sachkov_Hackathon
{
    public class Section
    {
        public string Type { get; init; }
        public int Number { get; init; }

        private readonly Rack[] _racks;
        private const int Capacity = 500;

        public Section(string type, int number)
        {
            Type = type;
            Number = number;
            _racks = new Rack[Capacity];
            for (int i = 0; i < Capacity; i++) 
            {
                _racks[i] = new Rack(i);
            }
        }
        public bool TryAddItem(Item item)
        {
            foreach (var rack in _racks)
            {
                if (rack.PutItem(item))
                    return true;
            }
            return false;
        }

        public bool TryRemoveItem(int itemId, out Item removed)
        {
            foreach (var rack in _racks)
            {
                var itm = rack.RemoveItem(itemId);
                if (itm != null)
                {
                    removed = itm;
                    return true;
                }
            }
            removed = null!;
            return false;
        }

        public Rack[] GetRacks()
        {
            var racks = _racks.Where(r => r != null).Select(r =>
            {
                var newRack = new Rack(r.Id);
                foreach (var item in r.GetItems())
                {
                    newRack.PutItem(item);
                }
                return newRack;
            })
                .ToArray();
            return racks;
        }
        
        public List<Item> GetItem(string type)
        {
            List<Item> items = new List<Item> ();

            foreach (var rack in _racks)
            {
                foreach (var rackItem in rack.GetItems())
                {
                    if (rackItem.Type == type)
                        items.Add(rackItem);
                }
            }
            return items;
        }

        public Item? GetItemById(int id)
        {
            Item? item = null;
            foreach(var rack in _racks)
            {
                item = rack.GetItem(id);
                if (item != null)
                    break;
            }
            return item;
        }

        public void RemoveItem(Item item)
        {
            Item? removedItem = null;
            foreach(var rack in _racks)
            {
                removedItem = rack.RemoveItem(item.Id);
                if (removedItem != null)
                    break;
            }
        }
    }
}