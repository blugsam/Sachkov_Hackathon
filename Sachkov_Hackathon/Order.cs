using System.Collections.Generic;
using static Sachkov_Hackathon.ItemDTO;

namespace Sachkov_Hackathon
{
    public enum OrderStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    public class Order
    {
        public int Id { get; init; }
        public List<ItemKey> ItemsToCollect { get; init; }

        public OrderStatus Status { get; set; }

        private static int _nextId = 1;

        public Order(List<ItemKey> items)
        {
            Id = _nextId++;
            ItemsToCollect = items;
            Status = OrderStatus.Pending;
        }

        public override string ToString()
        {
            return $"Заказ #{Id} ({ItemsToCollect.Count} поз., Статус: {Status})";
        }
    }

}