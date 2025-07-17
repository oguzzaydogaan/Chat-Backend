using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }
        private readonly ChatService _chatService;

        [HttpGet("{chatId}/users/{userId}")]
        public async Task<IActionResult> GetChatMessagesAsync(int userId, int chatId)
        {
            if (chatId <= 0)
                return BadRequest("Invalid chat ID.");
            try
            {
                var chat = await _chatService.GetChatMessagesAsync(chatId, userId);            
                return Ok(JsonSerializer.Serialize(chat));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateChatAsync([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count < 2)
                return BadRequest("You must enter at least two users to create a chat");
            try
            {
                var chat = await _chatService.AddChatAsync(userIds);
                return Created();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("{chatId}/users/{userId}")]
        public async Task<IActionResult> AddUserToChatAsync(int chatId, int userId)
        {
            if (chatId <= 0 || userId <= 0)
                return BadRequest("Invalid chat ID or user ID.");
            try
            {
                var updatedChat = await _chatService.AddUserToChatAsync(chatId, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChatAsync(int chatId)
        {
            if (chatId <= 0)
                return BadRequest("Invalid chat ID.");
            try
            {
                await _chatService.DeleteChatAsync(chatId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
