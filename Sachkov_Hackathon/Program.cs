using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sachkov_Hackathon.ItemDTO;

namespace Sachkov_Hackathon
{
    class Program
    {
        static readonly ConcurrentQueue<Item> incomingItems = new ConcurrentQueue<Item>();
        static readonly ConcurrentQueue<Order> pendingOrders = new ConcurrentQueue<Order>();
        static readonly List<Section> warehouseSections = new List<Section>();
        static readonly Random random = new Random();
        static int _itemIdCounter = 1;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Запуск симуляции 'Склад-Аквариум'...");

            InitializeWarehouse();
            Console.WriteLine($"Склад инициализирован: {warehouseSections.Count} секций.");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Остановка симуляции...");
                cts.Cancel();
                e.Cancel = true;
            };

            var collectorTasks = new List<Task>();
            int numberOfCollectors = 3;
            ConsoleColor[] colors = { ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta };

            for (int i = 0; i < numberOfCollectors; i++)
            {
                var collector = new Collector(i + 1, warehouseSections, incomingItems, pendingOrders, colors[i % colors.Length]);
                collectorTasks.Add(Task.Run(() => collector.RunAsync(cts.Token), cts.Token));
            }
            Console.WriteLine($"{numberOfCollectors} сборщиков запущено.");

            var generatorTasks = new List<Task>
            {
                Task.Run(() => GenerateIncomingItemsAsync(cts.Token)),
                Task.Run(() => GenerateOrdersAsync(cts.Token))
            };
            Console.WriteLine("Генераторы поставок и заказов запущены.");

            Console.WriteLine("Симуляция работает. Нажмите Ctrl+C для остановки.");

            try
            {
                await Task.WhenAll(generatorTasks);
                Console.WriteLine("Генераторы остановлены.");
                await Task.WhenAll(collectorTasks);
                Console.WriteLine("Сборщики остановлены.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Симуляция прервана.");
                try
                {
                    await Task.WhenAll(collectorTasks);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка во время ожидания: {ex.Message}");
            }


            Console.WriteLine("Симуляция завершена.");
        }


        static void InitializeWarehouse()
        {
            var toolsSection = new Section("Одежда", 1);

            var fastenersSection = new Section("Обувь", 2);

            warehouseSections.Add(toolsSection);
            warehouseSections.Add(fastenersSection);
        }


        static async Task GenerateIncomingItemsAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[ГЕНЕРАТОР ПОСТАВОК] Запущен.");
            string[] clothingItems = { "Футболка", "Джинсы", "Куртка", "Пальто", "Платье" };
            string[] shoesItems = { "Кроссовки", "Ботинки", "Туфли", "Сандалии", "Сапоги" };

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(random.Next(5, 11)), cancellationToken);

                int batchSize = random.Next(5, 16);
                for (int i = 0; i < batchSize; i++)
                {
                    string type = random.NextDouble() < 0.5 ? "Одежда" : "Обувь";
                    string name;
                    int price;

                    if (type == "Одежда")
                    {
                        name = clothingItems[random.Next(clothingItems.Length)] + $" {random.Next(38, 50)}";
                        price = random.Next(1000, 10000);
                    }
                    else
                    {
                        name = shoesItems[random.Next(shoesItems.Length)] + $" {random.Next(36, 46)}";
                        price = random.Next(2000, 15000);
                    }

                    var newItem = new Item(name, _itemIdCounter++, type, price);
                    incomingItems.Enqueue(newItem);
                    Console.WriteLine($"[ПОСТАВКА] + {newItem.Name} (Тип: {newItem.Type}, ID: {newItem.Id}) -> Очередь приемки ({incomingItems.Count})");
                }
            }
            Console.WriteLine("[ГЕНЕРАТОР ПОСТАВОК] Остановлен.");
        }

        static async Task GenerateOrdersAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[ГЕНЕРАТОР ЗАКАЗОВ] Запущен.");
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(random.Next(10, 21)), cancellationToken);

                List<Item> snapshot = warehouseSections
                    .SelectMany(s => s.GetRacks())
                    .SelectMany(r => r.GetItems())
                    .ToList();


                if (snapshot.Count > 0)
                {
                    int count = random.Next(3, Math.Min(9, snapshot.Count + 1));
                    List<ItemKey> keys = snapshot
                        .OrderBy(_ => random.Next())
                        .Take(count)
                        .Select(i => new ItemKey(i.Id, i.Type))
                        .ToList();

                    if (keys.Any())
                    {
                        var newOrder = new Order(keys);
                        pendingOrders.Enqueue(newOrder);
                        Console.WriteLine(
                          $"[ГЕНЕРАТОР ЗАКАЗОВ] + Заказ #{newOrder.Id} на {keys.Count} поз. -> Очередь сборки ({pendingOrders.Count})");
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }

            }
            Console.WriteLine("[ГЕНЕРАТОР ЗАКАЗОВ] Остановлен.");
        }
    }
}