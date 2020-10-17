using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace backend
{
    public class VkApi
    {
        private readonly string _access_token;
        private readonly HttpClient _client;
        private readonly ILogger<VkApi> _logger;
        private readonly string _v;

        public VkApi(ILogger<VkApi> logger, string accessToken, string v)
        {
            _client = new HttpClient();
            _logger = logger;
            _access_token = accessToken;
            _v = v;
        }

        public async Task<JObject> CallAsync(string method, Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("v"))
                parameters.Add("v", _v);
            if (!parameters.ContainsKey("access_token"))
                parameters.Add("access_token", _access_token);
            //
            var urlparameters = new FormUrlEncodedContent(parameters);

            try
            {
                var httpresult = await _client.PostAsync("https://api.vk.com/method/" + method, urlparameters);
                if (!httpresult.IsSuccessStatusCode)
                    return null;
                var response = await httpresult.Content.ReadAsStringAsync();
                return JObject.Parse(response);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Ошибка запроса к апи вк");
                return null;
            }
        }
    }
}