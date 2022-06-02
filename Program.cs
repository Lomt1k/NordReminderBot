using Newtonsoft.Json;
using NordDailyReminder.Networking;
using System;
using System.IO;
using System.Threading.Tasks;

/* 
 * Настраиваем сообщения в config.json
 * Чтобы узнать fileId - отправляем файл в личку нашему боту
 * 
 * Для расширения функционала все запросы можно глянуть тут:
 * https://myteam.mail.ru/botapi/
 */

namespace NordDailyReminder
{
    internal class Program
    {
        public static MyteamBotClient client { get; private set; }

        private static string configFilePath = "config.json";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"Запускаемся...");

            MainAsync();

            while (true)
                Console.ReadKey();
        }

        private static async void MainAsync()
        {
            if (!TryLoadConfig(out var config))
                return;

            client = new MyteamBotClient(config);
            var result = await client.GetBotInfo();
            if (!result.ok)
            {
                Console.WriteLine($"Указан некорректный токен в config.json");
                Console.WriteLine($"Настройте config.json и перезапустите приложение...");
                return;
            }
            Console.WriteLine($"Бот {result.data.firstName} успешно запущен");

            await RunAsync();
        }

        private static bool TryLoadConfig(out Config config)
        {
            config = new Config();
            if (!File.Exists(configFilePath))
            {
                var newConfigJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, newConfigJson);
                Console.WriteLine("Создан новый файл конфигурации");
                Console.WriteLine("Настройте config.json и перезапустите приложение...");
                Console.ReadKey();
                return false;
            }

            var configJson = File.ReadAllText(configFilePath);
            config = JsonConvert.DeserializeObject<Config>(configJson);
            return true;
        }

        private static async Task RunAsync()
        {
            while (true)
            {
                var dateTime = DateTime.Now;
                if (dateTime.DayOfWeek == DayOfWeek.Sunday || dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.Hour != 10)
                {
                    Console.WriteLine($"[{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}] Sleeping 30 minutes...");
                    await Task.Delay(1800 * 1000);                    
                    continue;
                }

                if (dateTime.Minute < 57)
                {
                    Console.WriteLine($"[{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}] Sleeping 10 seconds...");
                    await Task.Delay(10 * 1000);
                    continue;
                }

                SendRandomMessageAsync();
                await Task.Delay(1800 * 1000);
            }
        }

        private static async void SendRandomMessageAsync()
        {
            var messages = client.messages;
            if (messages.Length < 1)
                return;

            var index = new Random().Next(0, messages.Length);
            var message = messages[index];

            Console.WriteLine($"\nSending message with index {index}...");
            Console.WriteLine(message.text + "\n");
            if (string.IsNullOrEmpty(message.fileId))
            {
                await client.SendTextAsync(message.text);
                return;
            }

            await client.SendFileAsync(message.fileId, message.text);
        }

    }
}
