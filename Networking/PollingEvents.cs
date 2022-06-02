using Newtonsoft.Json;
using NordDailyReminder.Networking.ResponseData;
using System;
using System.Threading.Tasks;

namespace NordDailyReminder.Networking
{
    internal class PollingEvents
    {
        private const int pollTimeInSeconds = 1;

        private MyteamBotClient _client;
        private int _lastEventId;
        private string _token;

        public PollingEvents(MyteamBotClient client, string token)
        {
            _client = client;
            _token = token;
            PollingAsync();
        }

        private async void PollingAsync()
        {
            while (true)
            {
                await Task.Delay(pollTimeInSeconds * 1000);
                SendGetEventsAsync();
            }
        }

        private async void SendGetEventsAsync()
        {
            HttpParameter[] parameters = new HttpParameter[]
            {
                new HttpParameter("token", _token),
                new HttpParameter("lastEventId", _lastEventId.ToString()),
                new HttpParameter("pollTime", pollTimeInSeconds.ToString())
            };
            var result = await SimpleHttpClient.GetAsync(_client.API_URL + "/events/get", parameters);
            if (result == null)
                return;

            var jsonStr = await result.Content.ReadAsStringAsync();
            // uncomment this for debug json response:
            //Console.WriteLine(jsonStr);
            var responseData = JsonConvert.DeserializeObject<PollingEventsData>(jsonStr);
            if (!responseData.ok)
            {
                Console.WriteLine($"PollingEvents Error\n {responseData.description}");
                return;
            }

            foreach (var pollingEvent in responseData.events)
            {
                HandleEvent(pollingEvent);
            }
        }

        private void HandleEvent(PollingEventData data)
        {
            _lastEventId = data.eventId;
            Console.WriteLine();
            Console.WriteLine($"eventId: {data.eventId} type: {data.type}");

            var payload = data.payload;
            if (payload == null)
                return;

            var dateTime = UnixTimeStampToDateTime(payload.timestamp.Value);
            if (payload.text != null)
            {
                Console.Write($"[{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}] ");
                Console.WriteLine($"{payload.from.firstName} {payload.from.lastName}: {payload.text}");
            }

            var parts = data.payload.parts;
            if (parts == null || parts.Length < 1)
                return;

            foreach (var part in parts)
            {
                var partPayload = part.payload;
                if (partPayload == null)
                    continue;

                if (partPayload.fileId != null)
                {
                    Console.Write($"\n[{dateTime.ToShortDateString()} {dateTime.ToLongTimeString()}] ");
                    Console.WriteLine($"{payload.from.firstName} {payload.from.lastName} прислал файл.");
                    Console.WriteLine($"Тип файла: {partPayload.type}");
                    Console.WriteLine($"fileId: {partPayload.fileId}");

                    _client.SendTextAsync($"fileId: {partPayload.fileId}", 
                        replyMessageId: payload.msgId, 
                        specialChatId: payload.chat.chatId);
                }
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


    }
}
