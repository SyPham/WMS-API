using Data.ViewModel.Line;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class LineService : ILineService
    {
        private IConfiguration _config;
        private readonly string _notifyUrl;
        private readonly string _authorizeUrl;
        private readonly string _tokenUrl;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _state;
        private readonly string _successUri;
        public LineService(IConfiguration config)
        {
            _config = config;
            var lineConfig = _config.GetSection("LineNotifyConfig");
            _notifyUrl = lineConfig.GetValue<string>("notifyUrl");

        }

        public async Task SendMessage(MessageParams msg)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + msg.Token);

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", msg.Message)
                });

                var response= await client.PostAsync("", form);
                var data = await response.Content.ReadAsStringAsync();

            }
        }

        public async Task SendWithPicture(MessageParams msg)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + msg.Token);

                var form = new MultipartFormDataContent
                {
                    {new StringContent(msg.Message), "message"},
                    {new ByteArrayContent(await new HttpClient().GetByteArrayAsync(msg.FileUri)), "imageFile", msg.Filename}
                };

                await client.PostAsync("", form);
            }

        }

        public async Task SendWithSticker(MessageParams msg)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + msg.Token);
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", msg.Message),
                    new KeyValuePair<string, string>("stickerPackageId", msg.StickerPackageId),
                    new KeyValuePair<string, string>("stickerId", msg.StickerId)
                });
                var response = await client.PostAsync("", form);
                var data = await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> FetchToken(string code)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_tokenUrl);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });
                var response = await client.PostAsync("", content);
                var data = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(data)["access_token"].ToString();
            }
        }
    }
}
