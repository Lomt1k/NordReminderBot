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
 * 
 * --- fileId персонажей ZeroCity:
 * Джина: 02q4a000cPe15Z6566MQ536298cf211bc
 * Джина одобряет: 02q4a000XjX9wEXcFNob6z6298cf3c1bc
 * Джина напугана: 02q4a000mcNJiNtlg9Wj4o6298cf5e1bc
 * Зомби-помощник: 02q35000ppzDAQYjVHAtLg6298cf7b1bc
 * Доктор: 02q3F000wbeZgTPJ1ocKby6298cfa31bc
 * Доктор напугана: 02q3F000dbd1A6zYq4IUah6298cfbe1bc
 * Техник Орландо: 02q2S000He27W2ShWaaQCY6298cfe41bc
 * Памела: 02q3I000PC1x0HRJliRsii6298cffa1bc
 * Старший: 02q3b000xTBlGxEKAnaRUx6298d0151bc
 * Торгаш со шпионского рынка: 02q3w000ZXToTSIiTM8DHp6298d02e1bc
 * 
 */

namespace NordDailyReminder
{
    internal class Program
    {
        public static MyteamBotClient client { get; private set; }

        private static string configFilePath = "config.json";
        private static int previousMessageIndex = -1;

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
            var result = await client.GetBotInfoAsync();
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
                var dt = DateTime.Now;
                var targetTime = client.config.targetTime;

                if (dt.DayOfWeek == DayOfWeek.Sunday || dt.DayOfWeek == DayOfWeek.Saturday || dt.Hour != targetTime.hour)
                {
                    var secondsToNewHour = (60 - dt.Minute) * 60;
                    await SleepAsync(secondsToNewHour);
                    continue;
                }

                if (dt.Minute < targetTime.minute || (dt.Minute == targetTime.minute && dt.Second < targetTime.second))
                {
                    await SleepAsync(10);
                    continue;
                }

                SendRandomMessageAsync();
                await SleepAsync(3600);
            }
        }

        private static async Task SleepAsync(int seconds)
        {
            var dateTime = DateTime.Now;
            var minutes = seconds / 60;
            var timeStr = minutes > 0 ? $"{minutes} minutes" : $"{seconds} seconds";
            Console.WriteLine($"[{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}] Sleeping {timeStr}...");
            await Task.Delay(seconds * 1000);
        }

        private static async Task SendRandomMessageAsync()
        {
            var messages = client.config.messages;
            if (messages.Length < 1)
                return;

            var index = previousMessageIndex;
            if (messages.Length == 1)
            {
                index = 0;
            }
            else while (index == previousMessageIndex)
            {
                index = new Random().Next(0, messages.Length);
            }            
            var message = messages[index];

            Console.WriteLine($"\nSending message with index {index}...");
            Console.WriteLine(message.text + "\n");
            if (!string.IsNullOrEmpty(message.fileId))
            {
                await client.SendFileAsync(client.targetChatId, message.fileId);
            }
            await client.SendTextAsync(client.targetChatId, message.text);
            previousMessageIndex = index;
        }

    }
}
