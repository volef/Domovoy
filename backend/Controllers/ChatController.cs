using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using BookmakerHouse.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace backend.Controllers
{
    [ApiController]
    public class ChatController : Controller
    {
        //private static Chat _testchat1 = new Chat {Id = 222, lat = 23.0m, @long = 55.0m, UserCount = 666};
        private readonly VkApi _api;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(VkApi api, DatabaseContext dbContext, ILogger<ChatController> logger)
        {
            _api = api;
            _dbContext = dbContext;
            _logger = logger;
        }

        
        /// <summary>
        /// Создать чат
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     POST /create
        ///     {
        ///          "name": "string",
        ///          "lat": 0,
        ///          "long": 0,
        ///          "adress": "string"
        ///     }
        ///
        /// </remarks>
        /// <returns>Созданная беседа</returns>
        /// <response code="201">Все ок, беседа создана</response>
        /// <response code="400">Ошибка парса ответа от апи вк</response>
        /// <response code="406">Ошибка выполнения запроса на апи вк</response> 
        [HttpPost]
        [Route("[controller]/create")]
        public async Task<IActionResult> Create(Chat chat)
        {
            var @params = new Dictionary<string, string>
            {
                {"user_ids", string.Empty},
                {"title", chat.Name}
            };
            var result = await _api.CallAsync("messages.createChat", @params);
            if (result == null)
                return StatusCode(406);
            if (!result.TryGetValue("response", out var response))
            {
                var error = result.GetValue("error").ToString();
                _logger.LogWarning(error);
                return BadRequest(error);
            }

            chat.Id = response.Value<int>();
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
            return StatusCode(201, chat);
        }

        /// <summary>
        /// Получить инвайт-ссылку на беседу
        /// </summary>
        /// <param name="id">Id беседы</param>
        /// <param name="reset">Пересоздавать ли ссылку</param>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     POST /create
        ///     {
        ///          "name": "string",
        ///          "lat": 0,
        ///          "long": 0,
        ///          "adress": "string"
        ///     }
        ///
        /// </remarks>
        /// <returns>Ссылка-приглашение в беседу</returns>
        /// <response code="200">Все ок</response>
        /// <response code="404">Ошибка от апи вк</response>
        /// <response code="406">Ошибка выполнения запроса на апи вк</response> 
        [HttpGet]
        [Route("[controller]/getInviteLink")]
        public async Task<IActionResult> GetInviteLink(int id, bool reset = false)
        {
            var @params = new Dictionary<string, string>
            {
                {"peer_id", (2000000000 + id).ToString()},
                {"reset", reset ? "1" : "0"}
            };
            var result = await _api.CallAsync("messages.getInviteLink", @params);
            if (result == null)
                return StatusCode(406);
            if (!result.TryGetValue("response", out var response))
            {
                var error = result.GetValue("error");
                _logger.LogError(error.ToString());
                return NotFound(error["error_msg"].Value<string>());
            }

            var link = response.Value<string>("link");
            return StatusCode(200, link);
        }

        /// <summary>
        /// Поиск по адресу
        /// </summary>
        /// <param name="adress">Адрес</param>
        /// <remarks>
        /// Пример запроса:
        ///
        ///     POST /create
        ///     {
        ///          "name": "string",
        ///          "lat": 0,
        ///          "long": 0,
        ///          "adress": "string"
        ///     }
        ///
        /// </remarks>
        /// <returns>Список бесед</returns>
        /// <response code="200">Все ок</response>
        /// <response code="404">Не найдено ничего</response>
        /// <response code="406">Ошибка обновления данных о чатах</response> 
        [HttpPost]
        [Route("[controller]/getByAdress")]
        public async Task<IActionResult> GetByAdress(string adress)
        {
            var result = await _dbContext.Chats
                .Where(a => a.Adress == adress)
                .ToArrayAsync();
            if (result == null)
                return NotFound();
            if(!await UpdateChats(result))
                return StatusCode(406);
            return Ok(result);
        }

        //LOAD CHATS ONLY EXISTED IN BASE
        [NonAction]
        public async Task<bool> UpdateChats(Chat[] chats)
        {
            if(chats==null || !chats.Any())
                return true;
            //
            var @params = new Dictionary<string, string>
            {
                {"peer_ids", string.Join(",",chats.Select(c => 2000000000 + c.Id))},
                {"extended", "1"}
            };
            var result = await _api.CallAsync("messages.getConversationsById", @params);
            if (result == null || !result.TryGetValue("response", out var response))
            {
                var error = result?.GetValue("error").ToString();
                _logger.LogWarning(error);
                return false;
            }

            try
            {
                for (int i = 0; i < chats.Length; i++)
                {
                    chats[i].Name = response["items"][i]["chat_settings"]["title"].Value<string>();
                    chats[i].UserCount = response["items"][i]["chat_settings"]["members_count"].Value<int>();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Ошибка синхронизации данных о чатах");
                return false;
            }
            _dbContext.Chats.UpdateRange(chats);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}