using Newtonsoft.Json;
using NordDailyReminder.Networking.ResponseData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NordDailyReminder.Networking
{
    internal struct GetBotInfoReslult
    {
        public bool ok { get; }
        public BotData? data { get; }

        public GetBotInfoReslult(bool _ok, BotData _data)
        {
            ok = _ok;
            data = _data;
        }

        public static GetBotInfoReslult error = new GetBotInfoReslult(false, null);
    }

    internal struct SendMessageResult
    {
        public bool ok { get; }
        public string? messageId { get; }

        public SendMessageResult(bool _ok, string? _messageId)
        {
            ok = _ok;
            messageId = _messageId;
        }

        public static SendMessageResult error = new SendMessageResult(false, null);
    }

    internal class MyteamBotClient
    {
        private Config _config;
        private PollingEvents _polling;

        public string token => _config.token;
        public string targetChatId => _config.chatId;
        public string API_URL => _config.API_URL;
        public Config config => _config;

        public MyteamBotClient(Config config)
        {
            _config = config;
            _polling = new PollingEvents(this, token);
        }

        public async Task<GetBotInfoReslult> GetBotInfoAsync()
        {
            HttpParameter[] parameters = new HttpParameter[]
            {
                new HttpParameter("token", token),
            };
            var result = await SimpleHttpClient.GetAsync(API_URL + "/self/get", parameters);
            if (result == null)
            {
                return GetBotInfoReslult.error;
            }

            var jsonStr = await result.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<BotData>(jsonStr);
            if (!responseData.ok)
            {
                Console.WriteLine($"GetBotInfo Error\n {responseData.description}");
                return GetBotInfoReslult.error;
            }

            return new GetBotInfoReslult(true, responseData);
        }

        public async Task<SendMessageResult> SendTextAsync(string chatId, string text, string? replyMessageId = null)
        {
            List<HttpParameter> parameters = new List<HttpParameter>
            {
                new HttpParameter("token", token),
                new HttpParameter("chatId", chatId),
                new HttpParameter("text", text)
            };
            if (replyMessageId != null)
            {
                parameters.Add(new HttpParameter("replyMsgId", replyMessageId));
            }

            var result = await SimpleHttpClient.GetAsync(API_URL + "/messages/sendText", parameters);
            if (result == null)
            {
                return SendMessageResult.error;
            }

            var jsonStr = await result.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<SendedMessageData>(jsonStr);
            if (!responseData.ok)
            {
                Console.WriteLine($"SendText Error\n {responseData.description}");
                return SendMessageResult.error;
            }

            return new SendMessageResult(true, responseData.msgId);
        }

        public async Task<SendMessageResult> SendFileAsync(string chatId, string fileId, string? caption = null)
        {
            List<HttpParameter> parameters = new List<HttpParameter>
            {
                new HttpParameter("token", token),
                new HttpParameter("chatId", chatId),
                new HttpParameter("fileId", fileId)
            };
            if (caption != null)
            {
                parameters.Add(new HttpParameter("caption", caption));
            }

            var result = await SimpleHttpClient.GetAsync(API_URL + "/messages/sendFile", parameters);
            if (result == null)
                return SendMessageResult.error;

            var jsonStr = await result.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<SendedMessageData>(jsonStr);
            if (!responseData.ok)
            {
                Console.WriteLine($"SendFile Error\n {responseData.description}");
                return SendMessageResult.error;
            }

            return new SendMessageResult(true, responseData.msgId);
        }

    }
}
