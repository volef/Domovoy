using System.Collections.Generic;
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

        [HttpPost]
        [Route("[controller]/create")]
        public async Task<IActionResult> Create(Chat chat)
        {
            var @params = new Dictionary<string, string>
            {
                {"user_ids", chat.CreatorId.ToString()},
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
                var error = result.GetValue("error").ToString();
                _logger.LogWarning(error);
                return NotFound(error);
            }

            var link = response.Value<string>("link");
            return StatusCode(200, link);
        }

        [HttpPost]
        [Route("[controller]/getByAdress")]
        public async Task<IActionResult> GetByAdress(string adress)
        {
            var result = await _dbContext.Chats
                .SingleOrDefaultAsync(a => a.Adress == adress);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}