using System.Collections.Concurrent;

namespace Sachkov_Hackathon
{
    public class Collector
    {
        public int Id { get; init; }
        private readonly List<Section> _sections;
        private readonly ConcurrentQueue<Item> _incomingItemsQueue;
        private readonly ConcurrentQueue<Order> _ordersQueue;
        private static readonly Random _random = Random.Shared;
        private readonly ConsoleColor _logColor;
        private static readonly object _consoleLock = new object();

        public Collector(int id, List<Section> sections, ConcurrentQueue<Item> incomingItemsQueue, ConcurrentQueue<Order> ordersQueue, ConsoleColor color)
        {
            Id = id;
            _sections = sections;
            _incomingItemsQueue = incomingItemsQueue;
            _ordersQueue = ordersQueue;
            _logColor = color;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Log($"Запущен.");
            while (!cancellationToken.IsCancellationRequested)
            {
                bool workDone = false;

                if (_ordersQueue.TryDequeue(out Order orderToProcess))
                {
                    workDone = true;
                    Log($"Взял в работу {orderToProcess}");
                    await ProcessOrderAsync(orderToProcess, cancellationToken);
                }

                else if (_incomingItemsQueue.TryDequeue(out Item itemToPlace))
                {
                    workDone = true;
                    Log($"Взял на размещение {itemToPlace.Name} (ID: {itemToPlace.Id})");
                    await PlaceItemAsync(itemToPlace, cancellationToken);
                }


                if (!workDone)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 + _random.Next(100)), cancellationToken);
                }
            }
            Log($"Остановлен.");
        }

        private async Task PlaceItemAsync(Item item, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 3)), cancellationToken);

                var targetSection = _sections.FirstOrDefault(s => s.Type == item.Type);
                if (targetSection == null)
                {
                    Log($"Ошибка: Не найдена секция типа '{item.Type}' для {item.Name}");
                    _incomingItemsQueue.Enqueue(item);
                    await Task.Delay(1000, cancellationToken);
                    return;
                }

                if (targetSection.TryAddItem(item))
                {
                    Log($"Разместил {item.Name} (ID:{item.Id}) в секции {targetSection.Number}");
                }
                else
                {
                    Log($"Ошибка: Нет места для {item.Name} (ID:{item.Id}) в секции {targetSection.Number}");
                    _incomingItemsQueue.Enqueue(item);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Log($"Операция размещения отменена.");
            }
            catch (Exception ex)
            {
                Log($"Критическая ошибка при размещении {item.Name}: {ex.Message}");
                _incomingItemsQueue.Enqueue(item);
            }
        }

        private async Task ProcessOrderAsync(Order order, CancellationToken cancellationToken)
        {
            try
            {
                order.Status = OrderStatus.InProgress;
                bool allItemsFound = true;

                foreach (var key in order.ItemsToCollect)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(3, 8)), cancellationToken);

                    var section = _sections.FirstOrDefault(s => s.Type == key.Type);
                    if (section == null)
                    {
                        Log($"Ошибка: секция типа '{key.Type}' не найдена для заказа #{order.Id}");
                        allItemsFound = false;
                        break;
                    }

                    if (section.TryRemoveItem(key.Id, out var realItem))
                    {
                        Log($"Взял {realItem.Name} (ID:{realItem.Id}) для заказа #{order.Id} из секции {section.Number}");
                    }
                    else
                    {
                        Log($"Не найден товар ID:{key.Id} (тип {key.Type}) для заказа #{order.Id}");
                        allItemsFound = false;
                        break;
                    }
                }

                if (allItemsFound)
                {
                    order.Status = OrderStatus.Completed;
                    Log($"Завершил {order}");
                }
                else
                {
                    order.Status = OrderStatus.Failed;
                    Log($"Не удалось собрать {order} — не все товары найдены.");
                }
            }
            catch (OperationCanceledException)
            {
                order.Status = OrderStatus.Failed;
                Log($"Сборка заказа #{order.Id} отменена.");
            }
            catch (Exception ex)
            {
                order.Status = OrderStatus.Failed;
                Log($"Критическая ошибка при сборке заказа #{order.Id}: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            lock (_consoleLock)
            {
                Console.ForegroundColor = _logColor;
                Console.WriteLine($"[Сборщик {Id,2}] {DateTime.Now:HH:mm:ss.fff}: {message}");
                Console.ResetColor();
            }
        }
    }
}