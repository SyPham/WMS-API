using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BotSignalr.Models;
using System.Net.Http;
using BotSignalr.ConfigLine;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotSignalr.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        /// <summary>取得使用者 Token</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <returns></returns>
        private async Task<string> FetchToken(string code)
        {
            var _tokenUrl = "https://notify-bot.line.me/oauth/token";
            var lineNotifyConfig = _configuration.GetSection("LineNotifyConfig").Get<LineNotifyConfig>();

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_tokenUrl);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri",lineNotifyConfig.redirect_uri),
                    new KeyValuePair<string, string>("client_id", lineNotifyConfig.client_id),
                    new KeyValuePair<string, string>("client_secret", lineNotifyConfig.client_secret)
                });
                var response = await client.PostAsync("", content);
                var data = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<JObject>(data)["access_token"].ToString();
            }
        }
        // GET: api/LineNotify/SendMessage?target=PoyChang&message=HelloWorld
        /// <summary>傳送文字訊息</summary>
        /// <param name="token">令牌</param>
        /// <param name="message">訊息</param>
        [HttpGet]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string token, string message)
        {
            var _notifyUrl = "https://notify-bot.line.me/oauth/token";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var form = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("message", message)
        });

                await client.PostAsync("", form);
            }

            return new EmptyResult();
        }
        public async Task<IActionResult> Notify(string code, string state)
        {
          
            ViewBag.Code = code;
            var lineNotifyConfig = _configuration.GetSection("LineNotifyConfig").Get<LineNotifyConfig>();

            var url = "https://notify-bot.line.me/oauth/token";
            lineNotifyConfig.code = code;
            using (var client = new HttpClient())
            {
                // tin nhắn sẽ được thông báo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", lineNotifyConfig.grant_type },
                    { "code", code },
                    { "redirect_uri", lineNotifyConfig.redirect_uri },
                    { "client_id", lineNotifyConfig.client_id },
                    { "client_secret", lineNotifyConfig.client_secret },
                });
                // thêm mã token vào header
                var content2=  new StringContent(JsonConvert.SerializeObject(lineNotifyConfig),
                                           Encoding.UTF8,
                                           "application/x-www-form-urlencoded");//CONTENT-TYPE header
                // Thực hiện gửi thông báo
                var result = await client.PostAsync(url, content2);
                if (result.IsSuccessStatusCode)
                {
                    string res = result.Content.ReadAsStringAsync().Result;
                    return View();


                }
                return View();


                // Luu token notifi vao db
            }
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<JsonResult> AuthToken(string code)
        {
            var url = "https://notify-bot.line.me/oauth/token";
            var lineNotifyConfig = _configuration.GetSection("LineNotifyConfig").Get<LineNotifyConfig>();
            lineNotifyConfig.code = code;
            using (var client = new HttpClient())
            {
                // tin nhắn sẽ được thông báo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", lineNotifyConfig.grant_type },
                    { "code", code },
                    { "redirect_uri", lineNotifyConfig.redirect_uri },
                    { "client_id", lineNotifyConfig.client_id },
                    { "client_secret", lineNotifyConfig.client_secret },
                });
                // thêm mã token vào header
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                // Thực hiện gửi thông báo
                var result = await client.PostAsync(url, content);
                if (result.IsSuccessStatusCode)
                {
                    string res = result.Content.ReadAsStringAsync().Result;
                    return new JsonResult(res);

                }
                return new JsonResult(result);

                // Luu token notifi vao db
            }
            // return View();
        }
        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
